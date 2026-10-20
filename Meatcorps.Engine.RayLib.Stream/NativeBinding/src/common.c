#include "common.h"
#include <gst/rtsp-server/rtsp-server.h>
#include <glib-object.h>

// Shared output implementation. The platform backend submits encoded H.264/H.265
// access units; it never needs to know whether they are recorded or broadcast.
struct MCOutputState {
    MCOutput mode;

    // MP4 recording
    GstElement *pipeline;
    GstElement *file_source; // borrowed; owned by pipeline
    GstElement *file_audio_source; // borrowed from MP4 pipeline
    GstElement *live_audio_source; // held reference
    GstBus *bus;

    // RTSP: one shared media pipeline serves all clients.
    GstRTSPServer *server;
    GstRTSPMediaFactory *factory; // borrowed; owned by mount points
    GstRTSPMedia *media;          // held reference
    GstElement *live_source;      // held reference; NULL without a client
    GMainContext *context;
    GMainLoop *loop;
    GSource *server_source;
    GThread *thread;
    GMutex lock;
    GstClockTime first_pts;
    gboolean waiting_for_keyframe;
};

static const char *codec_caps(MCCodec codec) {
    return codec == MC_CODEC_H265 ? "video/x-h265" : "video/x-h264";
}

static const char *parser_name(MCCodec codec) {
    return codec == MC_CODEC_H265 ? "h265parse" : "h264parse";
}

static const char *payloader_name(MCCodec codec) {
    return codec == MC_CODEC_H265 ? "rtph265pay" : "rtph264pay";
}

static GstCaps *create_caps(const MCStream *stream) {
    return gst_caps_new_simple(codec_caps(stream->config.codec),
        "stream-format", G_TYPE_STRING, "byte-stream",
        "alignment", G_TYPE_STRING, "au",
        "width", G_TYPE_INT, stream->config.width,
        "height", G_TYPE_INT, stream->config.height,
        "framerate", GST_TYPE_FRACTION, stream->config.fps, 1,
        NULL);
}

static void configure_source(const MCStream *stream, GstElement *source,
                             gboolean recording) {
    g_object_set(source,
        "is-live", TRUE, "format", GST_FORMAT_TIME,
        "block", recording,
        "max-buffers", (guint64)12,
        "max-bytes", (guint64)(8 * 1024 * 1024),
        "leaky-type", recording ? GST_APP_LEAKY_TYPE_NONE
                                 : GST_APP_LEAKY_TYPE_DOWNSTREAM,
        NULL);
    GstCaps *caps = create_caps(stream);
    gst_app_src_set_caps(GST_APP_SRC(source), caps);
    gst_caps_unref(caps);
}

static gboolean audio_enabled(const MCStream *stream) {
    return stream->config.audio_sample_rate > 0;
}

static const char *aac_encoder_name(void) {
    static const char *names[] = {
        "avenc_aac",
        "fdkaacenc",
        "voaacenc"
    };

    for (guint i = 0; i < G_N_ELEMENTS(names); ++i) {
        GstElementFactory *factory =
            gst_element_factory_find(names[i]);

        if (factory) {
            gst_object_unref(factory);
            return names[i];
        }
    }

    return NULL;
}

static GstCaps *create_audio_caps(const MCStream *stream) {
    return gst_caps_new_simple(
        "audio/x-raw",
        "format", G_TYPE_STRING,
            G_BYTE_ORDER == G_LITTLE_ENDIAN
                ? "F32LE" : "F32BE",
        "layout", G_TYPE_STRING, "interleaved",
        "rate", G_TYPE_INT,
            stream->config.audio_sample_rate,
        "channels", G_TYPE_INT, 2,
        NULL
    );
}

static void configure_audio_source(
    const MCStream *stream,
    GstElement *source,
    gboolean recording
) {
    g_object_set(
        source,
        "is-live", TRUE,
        "format", GST_FORMAT_TIME,
        "block", recording,
        "max-bytes", (guint64)(1024 * 1024),
        "leaky-type",
            recording ? GST_APP_LEAKY_TYPE_NONE
                      : GST_APP_LEAKY_TYPE_DOWNSTREAM,
        NULL
    );

    GstCaps *caps = create_audio_caps(stream);

    gst_app_src_set_caps(
        GST_APP_SRC(source), caps
    );

    gst_caps_unref(caps);
}

static MCResult start_mp4(MCStream *stream) {
    MCOutputState *o = stream->output;

    o->pipeline = gst_pipeline_new("meatcorps-recorder");

    GstElement *video = gst_element_factory_make(
        "appsrc", "encoded-source"
    );

    GstElement *parser = gst_element_factory_make(
        parser_name(stream->config.codec), "video-parser"
    );

    GstElement *mux = gst_element_factory_make(
        "mp4mux", "mux"
    );

    GstElement *sink = gst_element_factory_make(
        "filesink", "sink"
    );

    if (!o->pipeline || !video || !parser || !mux || !sink) {
        mc_set_error(stream, "Missing MP4 video elements");

        if (video) gst_object_unref(video);
        if (parser) gst_object_unref(parser);
        if (mux) gst_object_unref(mux);
        if (sink) gst_object_unref(sink);

        return MC_ERR_GSTREAMER;
    }

    g_object_set(
        sink,
        "location", stream->config.location,
        NULL
    );

    g_object_set(mux, "faststart", TRUE, NULL);

    configure_source(stream, video, TRUE);

    gst_bin_add_many(
        GST_BIN(o->pipeline),
        video, parser, mux, sink, NULL
    );

    o->file_source = video;

    if (!gst_element_link_many(
        video, parser, mux, sink, NULL
    )) {
        mc_set_error(stream, "Could not link MP4 video branch");
        return MC_ERR_GSTREAMER;
    }

    if (audio_enabled(stream)) {
        const char *encoder_name = aac_encoder_name();

        if (!encoder_name) {
            mc_set_error(
                stream,
                "Missing AAC encoder: avenc_aac, "
                "fdkaacenc or voaacenc"
            );
            return MC_ERR_GSTREAMER;
        }

        GstElement *audio = gst_element_factory_make(
            "appsrc", "pcm-source"
        );

        GstElement *queue = gst_element_factory_make(
            "queue", "audio-queue"
        );

        GstElement *convert = gst_element_factory_make(
            "audioconvert", "audio-convert"
        );

        GstElement *resample = gst_element_factory_make(
            "audioresample", "audio-resample"
        );

        GstElement *encoder = gst_element_factory_make(
            encoder_name, "aac-encoder"
        );

        GstElement *aacparser = gst_element_factory_make(
            "aacparse", "aac-parser"
        );

        GstElement *filter = gst_element_factory_make(
            "capsfilter", "aac-caps"
        );

        if (!audio || !queue || !convert ||
            !resample || !encoder || !aacparser || !filter) {

            mc_set_error(
                stream, "Missing GStreamer MP4 audio elements"
            );

            if (audio) gst_object_unref(audio);
            if (queue) gst_object_unref(queue);
            if (convert) gst_object_unref(convert);
            if (resample) gst_object_unref(resample);
            if (encoder) gst_object_unref(encoder);
            if (aacparser) gst_object_unref(aacparser);
            if (filter) gst_object_unref(filter);

            return MC_ERR_GSTREAMER;
        }

        configure_audio_source(stream, audio, TRUE);

        g_object_set(
            encoder,
            "bitrate",
            stream->config.audio_bitrate_kbps * 1000,
            NULL
        );

        GstCaps *aac_caps = gst_caps_from_string(
            "audio/mpeg,mpegversion=(int)4,"
            "stream-format=(string)raw"
        );

        g_object_set(filter, "caps", aac_caps, NULL);
        gst_caps_unref(aac_caps);

        gst_bin_add_many(
            GST_BIN(o->pipeline),
            audio, queue, convert, resample,
            encoder, aacparser, filter,
            NULL
        );

        o->file_audio_source = audio;

        if (!gst_element_link_many(
            audio, queue, convert, resample,
            encoder, aacparser, filter, mux,
            NULL
        )) {
            mc_set_error(
                stream, "Could not link MP4 audio branch"
            );
            return MC_ERR_GSTREAMER;
        }
    }

    o->bus = gst_element_get_bus(o->pipeline);

    if (gst_element_set_state(
        o->pipeline, GST_STATE_PLAYING
    ) == GST_STATE_CHANGE_FAILURE) {
        mc_set_error(
            stream, "Could not start MP4 pipeline"
        );
        return MC_ERR_GSTREAMER;
    }

    return MC_OK;
}

static void media_unprepared(
    GstRTSPMedia *media,
    gpointer user_data
) {
    MCOutputState *o = user_data;

    GstRTSPMedia *old_media = NULL;
    GstElement *old_video = NULL;
    GstElement *old_audio = NULL;

    g_mutex_lock(&o->lock);

    if (o->media == media) {
        old_media = o->media;
        old_video = o->live_source;
        old_audio = o->live_audio_source;

        o->media = NULL;
        o->live_source = NULL;
        o->live_audio_source = NULL;

        o->first_pts = GST_CLOCK_TIME_NONE;
        o->waiting_for_keyframe = TRUE;
    }

    g_mutex_unlock(&o->lock);

    if (old_video) gst_object_unref(old_video);
    if (old_audio) gst_object_unref(old_audio);
    if (old_media) gst_object_unref(old_media);
}

static void media_configure(
    GstRTSPMediaFactory *factory,
    GstRTSPMedia *media,
    gpointer user_data
) {
    (void)factory;

    MCStream *stream = user_data;
    MCOutputState *o = stream->output;

    if (!o) return;

    GstElement *element =
        gst_rtsp_media_get_element(media);

    if (!element) return;

    GstElement *video =
        gst_bin_get_by_name_recurse_up(
            GST_BIN(element), "encoded-source"
        );

    GstElement *audio = NULL;

    if (audio_enabled(stream)) {
        audio = gst_bin_get_by_name_recurse_up(
            GST_BIN(element), "pcm-source"
        );
    }

    gst_object_unref(element);

    if (!video || (audio_enabled(stream) && !audio)) {
        if (video) gst_object_unref(video);
        if (audio) gst_object_unref(audio);
        return;
    }

    configure_source(stream, video, FALSE);

    if (audio)
        configure_audio_source(stream, audio, FALSE);

    g_signal_connect(
        media,
        "unprepared",
        G_CALLBACK(media_unprepared),
        o
    );

    g_mutex_lock(&o->lock);

    GstElement *old_video = o->live_source;
    GstElement *old_audio = o->live_audio_source;
    GstRTSPMedia *old_media = o->media;

    o->live_source = video;
    o->live_audio_source = audio;
    o->media = g_object_ref(media);

    o->first_pts = GST_CLOCK_TIME_NONE;
    o->waiting_for_keyframe = TRUE;

    g_mutex_unlock(&o->lock);

    if (old_video) gst_object_unref(old_video);
    if (old_audio) gst_object_unref(old_audio);
    if (old_media) gst_object_unref(old_media);
}

static gpointer server_thread(gpointer user_data) {
    MCOutputState *o = user_data;
    g_main_context_push_thread_default(o->context);
    g_main_loop_run(o->loop);
    g_main_context_pop_thread_default(o->context);
    return NULL;
}

static MCResult start_rtsp(MCStream *stream) {
    MCOutputState *o = stream->output;
    o->context = g_main_context_new();
    o->loop = g_main_loop_new(o->context, FALSE);
    o->server = gst_rtsp_server_new();
    g_mutex_init(&o->lock);
    o->first_pts = GST_CLOCK_TIME_NONE;
    o->waiting_for_keyframe = TRUE;

    gchar *port = g_strdup_printf("%u", (unsigned)stream->config.port);
    gst_rtsp_server_set_service(o->server, port);
    g_free(port);

    GstRTSPMountPoints *mounts = gst_rtsp_server_get_mount_points(o->server);
    GstRTSPMediaFactory *factory = gst_rtsp_media_factory_new();

    // RTSP media factory requires the payloader named pay0.
    gchar *launch = NULL;

    if (audio_enabled(stream)) {
        const char *encoder_name = aac_encoder_name();

        if (!encoder_name) {
            mc_set_error(
                stream,
                "Missing AAC encoder: avenc_aac, "
                "fdkaacenc or voaacenc"
            );
            return MC_ERR_GSTREAMER;
        }

        launch = g_strdup_printf(
            "( "
            // Video
            "appsrc name=encoded-source "
            "is-live=true format=time "
            "! queue max-size-buffers=4 "
            "max-size-bytes=0 max-size-time=0 "
            "leaky=downstream "
            "! %s "
            "! %s name=pay0 pt=96 config-interval=-1 "

            // Audio
            "appsrc name=pcm-source "
            "is-live=true format=time "
            "! queue max-size-time=100000000 "
            "max-size-bytes=0 max-size-buffers=0 "
            "leaky=downstream "
            "! audioconvert "
            "! audioresample "
            "! %s bitrate=%d "
            "! aacparse "
            "! audio/mpeg,mpegversion=4,stream-format=raw "
            "! rtpmp4gpay name=pay1 pt=97 "
            ")",

            parser_name(stream->config.codec),
            payloader_name(stream->config.codec),
            encoder_name,
            stream->config.audio_bitrate_kbps * 1000
        );
    } else {
        // Keep the proven video-only RTSP pipeline.
        launch = g_strdup_printf(
            "( appsrc name=encoded-source "
            "is-live=true format=time "
            "! queue max-size-buffers=4 "
            "max-size-bytes=0 max-size-time=0 "
            "leaky=downstream "
            "! %s "
            "! %s name=pay0 pt=96 config-interval=-1 )",

            parser_name(stream->config.codec),
            payloader_name(stream->config.codec)
        );
    }

    gst_rtsp_media_factory_set_launch(factory, launch);
    gst_rtsp_media_factory_set_shared(factory, TRUE);
    g_signal_connect(factory, "media-configure", G_CALLBACK(media_configure), stream);
    o->factory = factory; // mount owns it after add_factory
    gst_rtsp_mount_points_add_factory(mounts, stream->config.location, factory);
    gst_object_unref(mounts);
    g_free(launch);

    GError *error = NULL;
    o->server_source = gst_rtsp_server_create_source(o->server, NULL, &error);
    if (!o->server_source) {
        mc_set_error(stream, "RTSP bind/listen failed: %s",
                     error ? error->message : "unknown error");
        if (error) g_error_free(error);
        return MC_ERR_GSTREAMER;
    }
    if (!g_source_attach(o->server_source, o->context)) {
        mc_set_error(stream, "RTSP event source could not attach");
        return MC_ERR_GSTREAMER;
    }
    o->thread = g_thread_new("meatcorps-rtsp", server_thread, o);
    return MC_OK;
}

MCResult mc_output_start(MCStream *stream) {
    GError *error = NULL;
    if (!gst_init_check(NULL, NULL, &error)) {
        mc_set_error(stream, "gst_init failed: %s",
                     error ? error->message : "unknown error");
        if (error) g_error_free(error);
        return MC_ERR_GSTREAMER;
    }
    MCOutputState *o = g_new0(MCOutputState, 1);
    o->mode = stream->config.output;
    stream->output = o;
    return o->mode == MC_OUTPUT_MP4 ? start_mp4(stream) : start_rtsp(stream);
}

MCResult mc_output_push(MCStream *stream, GstBuffer *buffer) {
    MCOutputState *o = stream->output;
    if (!o) {
        gst_buffer_unref(buffer);
        return MC_ERR_STATE;
    }
    if (o->mode == MC_OUTPUT_MP4) {
        if (!o->file_source) {
            gst_buffer_unref(buffer);
            return MC_ERR_STATE;
        }
        GstFlowReturn flow = gst_app_src_push_buffer(
            GST_APP_SRC(o->file_source), buffer); // takes ownership
        return flow == GST_FLOW_OK ? MC_OK : MC_ERR_GSTREAMER;
    }

    GstElement *source = NULL;
    g_mutex_lock(&o->lock);
    if (o->live_source) {
        gboolean keyframe = !GST_BUFFER_FLAG_IS_SET(buffer, GST_BUFFER_FLAG_DELTA_UNIT);
        if (!o->waiting_for_keyframe || keyframe) {
            if (o->waiting_for_keyframe) {
                o->waiting_for_keyframe = FALSE;
                o->first_pts = GST_BUFFER_PTS(buffer);
            }
            source = gst_object_ref(o->live_source);
            if (GST_CLOCK_TIME_IS_VALID(o->first_pts)) {
                GstClockTime pts = GST_BUFFER_PTS(buffer);
                GstClockTime dts = GST_BUFFER_DTS(buffer);
                GST_BUFFER_PTS(buffer) = pts >= o->first_pts ? pts - o->first_pts : 0;
                if (GST_CLOCK_TIME_IS_VALID(dts))
                    GST_BUFFER_DTS(buffer) = dts >= o->first_pts ? dts - o->first_pts : 0;
            }
        }
    }
    g_mutex_unlock(&o->lock);

    // No receiver yet or waiting for a keyframe: discard compressed frame.
    if (!source) {
        gst_buffer_unref(buffer);
        return MC_OK;
    }
    GstFlowReturn flow = gst_app_src_push_buffer(GST_APP_SRC(source), buffer);
    gst_object_unref(source);
    // Connection may have vanished between acquisition and push.
    return (flow == GST_FLOW_OK || flow == GST_FLOW_FLUSHING)
        ? MC_OK : MC_ERR_GSTREAMER;
}

MCResult mc_output_push_audio(
    MCStream *stream,
    const float *samples,
    uint32_t frames,
    uint64_t pts_ns
) {
    MCOutputState *o = stream->output;

    if (!o || !audio_enabled(stream))
        return MC_ERR_STATE;

    gsize bytes = (gsize)frames * 2 * sizeof(float);

    GstBuffer *buffer = gst_buffer_new_allocate(
        NULL, bytes, NULL
    );

    if (!buffer)
        return MC_ERR_GSTREAMER;

    gst_buffer_fill(buffer, 0, samples, bytes);

    GST_BUFFER_PTS(buffer) = pts_ns;
    GST_BUFFER_DTS(buffer) = pts_ns;

    GST_BUFFER_DURATION(buffer) =
        gst_util_uint64_scale(
            frames,
            GST_SECOND,
            stream->config.audio_sample_rate
        );

    if (o->mode == MC_OUTPUT_MP4) {
        if (!o->file_audio_source) {
            gst_buffer_unref(buffer);
            return MC_ERR_STATE;
        }

        GstFlowReturn flow = gst_app_src_push_buffer(
            GST_APP_SRC(o->file_audio_source),
            buffer
        );

        return flow == GST_FLOW_OK
            ? MC_OK
            : MC_ERR_GSTREAMER;
    }

    // RTSP: audio and video must use the SAME
    // origin. The first video keyframe establishes
    // the timeline when an RTSP client connects.
    GstElement *source = NULL;

    g_mutex_lock(&o->lock);

    if (o->live_audio_source &&
        GST_CLOCK_TIME_IS_VALID(o->first_pts) &&
        pts_ns >= o->first_pts) {

        source = gst_object_ref(o->live_audio_source);

        GST_BUFFER_PTS(buffer) =
            pts_ns - o->first_pts;

        GST_BUFFER_DTS(buffer) =
            GST_BUFFER_PTS(buffer);
        }

    g_mutex_unlock(&o->lock);

    if (!source) {
        gst_buffer_unref(buffer);
        return MC_OK;
    }

    GstFlowReturn flow = gst_app_src_push_buffer(
        GST_APP_SRC(source),
        buffer
    );

    gst_object_unref(source);

    return (flow == GST_FLOW_OK ||
            flow == GST_FLOW_FLUSHING)
        ? MC_OK
        : MC_ERR_GSTREAMER;
}

MCResult mc_output_stop(MCStream *stream) {
    MCOutputState *o = stream->output;
    if (!o || o->mode == MC_OUTPUT_RTSP) return MC_OK;
    if (!o->file_source || !o->bus) return MC_OK;

    // MP4 needs EOS on every active track before
    // it can finalize the container properly.
    if (o->file_audio_source) {
        if (gst_app_src_end_of_stream(GST_APP_SRC(o->file_audio_source)) != GST_FLOW_OK) {
            mc_set_error(
                stream, "GStreamer rejected audio EOS"
            );
            return MC_ERR_GSTREAMER;
        }
    }

    if (gst_app_src_end_of_stream(GST_APP_SRC(o->file_source)) != GST_FLOW_OK) {
        mc_set_error(
            stream, "GStreamer rejected video EOS"
        );
        return MC_ERR_GSTREAMER;
    }
    GstMessage *msg = gst_bus_timed_pop_filtered(o->bus, 5 * GST_SECOND,
        GST_MESSAGE_EOS | GST_MESSAGE_ERROR);
    if (!msg) {
        mc_set_error(stream, "Timed out finalizing MP4");
        return MC_ERR_GSTREAMER;
    }
    MCResult result = MC_OK;
    if (GST_MESSAGE_TYPE(msg) == GST_MESSAGE_ERROR) {
        GError *error = NULL;
        gchar *debug = NULL;
        gst_message_parse_error(msg, &error, &debug);
        mc_set_error(stream, "GStreamer: %s",
            error ? error->message : "unknown error");
        if (error) g_error_free(error);
        g_free(debug);
        result = MC_ERR_GSTREAMER;
    }
    gst_message_unref(msg);
    return result;
}

void mc_output_destroy(MCStream *stream) {
    MCOutputState *o = stream->output;
    if (!o) return;

    if (o->mode == MC_OUTPUT_MP4) {
        if (o->pipeline) gst_element_set_state(o->pipeline, GST_STATE_NULL);
        if (o->bus) gst_object_unref(o->bus);
        if (o->pipeline) gst_object_unref(o->pipeline);
    } else if (o->context) {
        // Disconnect signals before freeing the callback's MCStream pointer.
        if (o->factory) g_signal_handlers_disconnect_by_data(o->factory, stream);
        g_mutex_lock(&o->lock);
        GstRTSPMedia *media = o->media;
        GstElement *source = o->live_source;
        GstElement *audio_source = o->live_audio_source;

        o->media = NULL;
        o->live_source = NULL;
        o->live_audio_source = NULL;
        g_mutex_unlock(&o->lock);

        if (media) g_signal_handlers_disconnect_by_data(media, o);
        if (o->server_source) {
            g_source_destroy(o->server_source);
            g_source_unref(o->server_source);
        }
        if (o->loop) g_main_loop_quit(o->loop);
        if (o->thread) g_thread_join(o->thread);
        if (source) gst_object_unref(source);
        if (audio_source)
            gst_object_unref(audio_source);
        if (media) gst_object_unref(media);
        if (o->server) gst_object_unref(o->server);
        if (o->loop) g_main_loop_unref(o->loop);
        g_main_context_unref(o->context);
        g_mutex_clear(&o->lock);
    }
    g_free(o);
    stream->output = NULL;
}
