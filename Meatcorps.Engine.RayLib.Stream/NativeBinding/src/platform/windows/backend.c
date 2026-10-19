/*
 * Meatcorps.VideoStreaming — Windows backend (first portable implementation)
 *
 * Preferred: GL texture -> GPU-side FBO copy -> GstGLMemory -> NVENC -> Annex B.
 * Fallback: GL readback -> CPU-visible GStreamer frame -> hardware encoder,
 *           or software encoder when hardware encoding cannot initialize.
 * Every fallback emits a prominent console warning; no silent path change.
 * Video/RTSP/MP4/AAC output is still handled by unchanged common.c.
 * The fast path is specifically NVIDIA/GLMemory, not a claim that VAAPI,
 * QSV or AMF support direct OpenGL input.
 * Requires GStreamer GL and Video libraries linked in CMake.
 * Start/Submit/Stop must run with the SAME GL context current.
 */
#include "backend.h"
#include "common.h"
#include <gst/app/gstappsrc.h>
#include <gst/app/gstappsink.h>
#include <gst/gl/gl.h>
#include <gst/video/video.h>
#include <glib.h>
#include <stdint.h>
#include <string.h>

#ifdef _WIN32
#  define MC_GL_CALL APIENTRY
#else
#  define MC_GL_CALL
#endif

#ifdef _WIN32
#include <windows.h>
#else
#include <dlfcn.h>
#endif

// Local OpenGL declarations avoid a dependency on GL extension headers.
typedef unsigned int MCGLuint;
typedef unsigned int MCGLenum;
typedef int MCGLint;
typedef int MCGLsizei;

#define MC_GL_VENDOR                    0x1F00
#define MC_GL_FRAMEBUFFER               0x8D40
#define MC_GL_DRAW_FRAMEBUFFER          0x8CA9
#define MC_GL_DRAW_FRAMEBUFFER_BINDING  0x8CA6
#define MC_GL_TEXTURE_BINDING_2D        0x8069
#define MC_GL_RGBA8                     0x8058
#define MC_GL_COLOR_BUFFER_BIT          0x00004000
#define MC_GL_NEAREST                   0x2600
#define MC_GL_TEXTURE_MIN_FILTER        0x2801
#define MC_GL_TEXTURE_MAG_FILTER        0x2800
#define MC_GL_NO_ERROR                  0
#define MC_GL_PACK_ROW_LENGTH           0x0D02
#define MC_GL_PACK_SKIP_ROWS            0x0D03
#define MC_GL_PACK_SKIP_PIXELS          0x0D04
#define MC_GL_READ_FRAMEBUFFER          0x8CA8
#define MC_GL_READ_FRAMEBUFFER_BINDING  0x8CAA
#define MC_GL_FRAMEBUFFER_COMPLETE      0x8CD5
#define MC_GL_COLOR_ATTACHMENT0         0x8CE0
#define MC_GL_TEXTURE_2D                0x0DE1
#define MC_GL_RGBA                      0x1908
#define MC_GL_UNSIGNED_BYTE             0x1401
#define MC_GL_PACK_ALIGNMENT            0x0D05
#define MC_GL_PIXEL_PACK_BUFFER         0x88EB
#define MC_GL_PIXEL_PACK_BUFFER_BINDING 0x88ED

typedef struct {
    void (MC_GL_CALL *GenFramebuffers)(MCGLsizei, MCGLuint *);
    void (MC_GL_CALL *DeleteFramebuffers)(MCGLsizei, const MCGLuint *);
    void (MC_GL_CALL *BindFramebuffer)(MCGLenum, MCGLuint);
    void (MC_GL_CALL *FramebufferTexture2D)(MCGLenum, MCGLenum, MCGLenum, MCGLuint, MCGLint);
    MCGLenum (MC_GL_CALL *CheckFramebufferStatus)(MCGLenum);
    void (MC_GL_CALL *GetIntegerv)(MCGLenum, MCGLint *);
    MCGLenum (MC_GL_CALL *GetError)(void);
    const unsigned char *(MC_GL_CALL *GetString)(MCGLenum);
    void (MC_GL_CALL *ReadPixels)(MCGLint, MCGLint, MCGLsizei, MCGLsizei,
                                 MCGLenum, MCGLenum, void *);
    void (MC_GL_CALL *PixelStorei)(MCGLenum, MCGLint);
    void (MC_GL_CALL *BindBuffer)(MCGLenum, MCGLuint);
    void (MC_GL_CALL *GenTextures)(MCGLsizei, MCGLuint *);
    void (MC_GL_CALL *DeleteTextures)(MCGLsizei, const MCGLuint *);
    void (MC_GL_CALL *BindTexture)(MCGLenum, MCGLuint);
    void (MC_GL_CALL *TexImage2D)(MCGLenum, MCGLint, MCGLint, MCGLsizei,
                                 MCGLsizei, MCGLint, MCGLenum, MCGLenum, const void *);
    void (MC_GL_CALL *TexParameteri)(MCGLenum, MCGLenum, MCGLint);
    void (MC_GL_CALL *BlitFramebuffer)(MCGLint, MCGLint, MCGLint, MCGLint,
                                      MCGLint, MCGLint, MCGLint, MCGLint,
                                      MCGLenum, MCGLenum);
    void (MC_GL_CALL *Finish)(void);
} GLFunctions;

enum { MC_GL_SLOTS = 4 };

typedef struct {
    MCGLuint texture;
    MCGLuint framebuffer;
    gint busy; // atomic; released on the GStreamer thread
} GPUSlot;

typedef struct Backend {
    MCStream *stream;
    void *context;
    MCGLuint framebuffer;
    GLFunctions gl;
#ifdef _WIN32
    HMODULE gl_library;
    HGLRC (WINAPI *wgl_get_current)(void);
    PROC (WINAPI *wgl_get_proc)(LPCSTR);
#else
    void *gl_library;
    void *egl_library;
    gboolean using_egl;
    void *(*glx_get_current)(void);
    void *(*egl_get_current)(void);
    void *(*glx_get_proc)(const unsigned char *);
    void *(*egl_get_proc)(const char *);
#endif
    GstElement *pipeline;
    GstElement *source;       // held reference
    GstElement *sink;         // held reference
    GstBus *bus;              // held reference
    gchar *encoder_name;
    gboolean gpu_mode;
    gboolean software_mode;
    GPUSlot slots[MC_GL_SLOTS];
    GstGLDisplay *gl_display;
    GstGLContext *gl_context;
    GstVideoInfo video_info;
    GstGLMemoryAllocator *gl_allocator;
    guint8 *flip_row;
    gsize row_bytes;
    gint64 next_frame_us;
    gint callback_error;     // atomic: encoded-output callback -> render thread
    gboolean encoder_started;
    gboolean output_started;
} Backend;

// Platform loader defined later in this file.
static gboolean platform_gl_open(Backend *b, MCStream *s);
static void *platform_gl_proc(Backend *b, const char *name);
static void platform_gl_close(Backend *b);
static void *platform_gl_current(Backend *b);

static gboolean load_gl(Backend *b, MCStream *s) {
#define LOAD_GL(member, symbol) do {                                   \
    void *address = platform_gl_proc(b, symbol);                        \
    _Static_assert(sizeof(b->gl.member) == sizeof(address),             \
                   "GL function pointer size mismatch");             \
    if (address) memcpy(&b->gl.member, &address, sizeof(address));      \
    if (!address) {                                                      \
        mc_set_error(s, "Missing OpenGL function: %s", symbol);        \
        return FALSE;                                                    \
    }                                                                   \
} while (0)
    LOAD_GL(GenFramebuffers, "glGenFramebuffers");
    LOAD_GL(DeleteFramebuffers, "glDeleteFramebuffers");
    LOAD_GL(BindFramebuffer, "glBindFramebuffer");
    LOAD_GL(FramebufferTexture2D, "glFramebufferTexture2D");
    LOAD_GL(CheckFramebufferStatus, "glCheckFramebufferStatus");
    LOAD_GL(GetIntegerv, "glGetIntegerv");
    LOAD_GL(GetError, "glGetError");
    LOAD_GL(GetString, "glGetString");
    LOAD_GL(ReadPixels, "glReadPixels");
    LOAD_GL(PixelStorei, "glPixelStorei");
    LOAD_GL(BindBuffer, "glBindBuffer");
    LOAD_GL(GenTextures, "glGenTextures");
    LOAD_GL(DeleteTextures, "glDeleteTextures");
    LOAD_GL(BindTexture, "glBindTexture");
    LOAD_GL(TexImage2D, "glTexImage2D");
    LOAD_GL(TexParameteri, "glTexParameteri");
    LOAD_GL(BlitFramebuffer, "glBlitFramebuffer");
    LOAD_GL(Finish, "glFinish");
#undef LOAD_GL
    return TRUE;
}

static gboolean factory_exists(const char *name) {
    GstElementFactory *f = gst_element_factory_find(name);
    if (!f) return FALSE;
    gst_object_unref(f);
    return TRUE;
}

// A deliberately small selection table; no platform enum in the public API.
typedef struct {
    MCEncoder choice;
    const char *h264;
    const char *h265;
} EncoderCandidate;

static const EncoderCandidate encoders[] = {
    { MC_ENCODER_NVENC, "nvh264enc", "nvh265enc" },
    { MC_ENCODER_AMF, "amfh264enc", "amfh265enc" },
    { MC_ENCODER_QSV, "qsvh264enc", "qsvh265enc" }
};

static MCEncoder preferred_for_gpu(Backend *b) {
    const char *vendor = (const char *)b->gl.GetString(MC_GL_VENDOR);
    if (!vendor) return MC_ENCODER_AUTO;
    if (g_strrstr(vendor, "NVIDIA")) return MC_ENCODER_NVENC;
    if (g_strrstr(vendor, "Intel")) return MC_ENCODER_QSV;
    if (g_strrstr(vendor, "AMD") || g_strrstr(vendor, "ATI"))
#ifdef _WIN32
        return MC_ENCODER_AMF;
#else
        return MC_ENCODER_VAAPI;
#endif
    return MC_ENCODER_AUTO;
}

static gboolean compatible_choice(MCEncoder choice) {
    if (choice == MC_ENCODER_AUTO) return TRUE;
    for (guint i = 0; i < G_N_ELEMENTS(encoders); ++i)
        if (encoders[i].choice == choice) return TRUE;
    return FALSE;
}

// Fallback only: software encoding when every hardware plugin is unavailable
// or cannot start.  Keep software-specific plugins out of hardware selection.
static const char *software_codec_name(MCCodec codec, guint index) {
    static const char *h264[] = { "x264enc", "openh264enc" };
    static const char *h265[] = { "x265enc" };
    return codec == MC_CODEC_H265
        ? (index < G_N_ELEMENTS(h265) ? h265[index] : NULL)
        : (index < G_N_ELEMENTS(h264) ? h264[index] : NULL);
}

static void warn_fallback(const char *platform, const char *why,
                          const char *encoder) {
    g_printerr("\n[Meatcorps.VideoStreaming][WARNING][%s] "
               "Fast GPU->NVENC path unavailable: %s\n"
               "[Meatcorps.VideoStreaming][WARNING][%s] "
               "FALLBACK: OpenGL readback to CPU memory -> %s encoder. "
               "Expect higher GPU/CPU load.\n\n",
               platform, why, platform, encoder);
}

// Avoid hardcoding signed/unsigned property types between encoder plugins.
static void set_int_property(GObject *object, const char *name, guint value) {
    GParamSpec *p = g_object_class_find_property(G_OBJECT_GET_CLASS(object), name);
    if (!p || !(p->flags & G_PARAM_WRITABLE)) return;
    if (G_PARAM_SPEC_VALUE_TYPE(p) == G_TYPE_UINT)
        g_object_set(object, name, value, NULL);
    else if (G_PARAM_SPEC_VALUE_TYPE(p) == G_TYPE_INT)
        g_object_set(object, name, (gint)value, NULL);
}

static void set_bool_property(GObject *object, const char *name, gboolean value) {
    GParamSpec *p = g_object_class_find_property(G_OBJECT_GET_CLASS(object), name);
    if (p && G_PARAM_SPEC_VALUE_TYPE(p) == G_TYPE_BOOLEAN && (p->flags & G_PARAM_WRITABLE))
        g_object_set(object, name, value, NULL);
}

static GstFlowReturn encoded_sample(GstAppSink *sink, gpointer user_data) {
    Backend *b = user_data;
    GstSample *sample = gst_app_sink_pull_sample(sink);
    if (!sample) return GST_FLOW_EOS;
    GstBuffer *buffer = gst_sample_get_buffer(sample);
    if (!buffer) {
        gst_sample_unref(sample);
        return GST_FLOW_ERROR;
    }

    // Writable metadata (PTS/DTS) with shared compressed payload memory.
    GstBuffer *packet = gst_buffer_copy(buffer);
    gst_sample_unref(sample);
    if (!packet) return GST_FLOW_ERROR;

    MCResult r = mc_output_push(b->stream, packet); // consumes packet
    if (r != MC_OK) {
        g_atomic_int_set(&b->callback_error, r);
        return GST_FLOW_ERROR;
    }
    return GST_FLOW_OK;
}

// Builds one internal RAW->HARDWARE ENCODER pipeline. It emits complete Annex-B
// access units into common.c; it does NOT start another RTSP server or AAC path.
static gboolean setup_encoder(Backend *b, MCStream *s, const char *name, gboolean gpu) {
    const char *parser = s->config.codec == MC_CODEC_H265 ? "h265parse" : "h264parse";
    const char *codec = s->config.codec == MC_CODEC_H265 ? "h265" : "h264";
    gboolean software = g_str_has_prefix(name, "x264enc") ||
        g_str_has_prefix(name, "x265enc") ||
        g_str_has_prefix(name, "openh264enc");
    gchar *conversion = gpu ? g_strdup("") : g_strdup_printf(
        "! videoconvert ! video/x-raw,format=%s",
        software ? "I420" : "NV12");
    gchar *desc = g_strdup_printf(
        "appsrc name=raw-source is-live=true format=time "
        "! queue max-size-buffers=4 max-size-bytes=0 max-size-time=0 leaky=downstream "
        "%s "
        "! %s name=hardware-encoder "
        "! %s config-interval=-1 "
        "! video/x-%s,stream-format=byte-stream,alignment=au "
        "! appsink name=encoded-sink sync=false emit-signals=true max-buffers=8",
        conversion, name, parser, codec);
    g_free(conversion);

    GError *err = NULL;
    b->pipeline = gst_parse_launch(desc, &err);
    g_free(desc);
    if (err) {
        mc_set_error(s, "Encoder pipeline (%s): %s", name, err->message);
        g_error_free(err);
        return FALSE;
    }
    if (!b->pipeline) {
        mc_set_error(s, "Failed to create encoder pipeline for %s", name);
        return FALSE;
    }

    // Tell downstream elements which Raylib OpenGL context owns the texture.
    // GStreamer can then establish an appropriate GL/CUDA sharing context.
    if (gpu) {
        GstContext *app = gst_context_new("gst.gl.app_context", TRUE);
        GstStructure *structure = gst_context_writable_structure(app);
        gst_structure_set(structure, "context", GST_GL_TYPE_CONTEXT,
                          b->gl_context, NULL);
        gst_element_set_context(b->pipeline, app);
        gst_context_unref(app);
    }

    b->source = gst_bin_get_by_name(GST_BIN(b->pipeline), "raw-source");
    b->sink = gst_bin_get_by_name(GST_BIN(b->pipeline), "encoded-sink");
    GstElement *encoder = gst_bin_get_by_name(GST_BIN(b->pipeline), "hardware-encoder");
    if (!b->source || !b->sink || !encoder) {
        if (encoder) gst_object_unref(encoder);
        mc_set_error(s, "Encoder pipeline missing required elements for %s", name);
        return FALSE;
    }

    set_int_property(G_OBJECT(encoder), "bitrate", (guint)s->config.bitrate_kbps);
    set_int_property(G_OBJECT(encoder), "gop-size", (guint)s->config.fps);
    set_int_property(G_OBJECT(encoder), "key-int-max", (guint)s->config.fps);
    set_int_property(G_OBJECT(encoder), "max-keyframe-distance", (guint)s->config.fps);
    set_int_property(G_OBJECT(encoder), "bframes", 0);
    set_int_property(G_OBJECT(encoder), "b-frames", 0);
    set_bool_property(G_OBJECT(encoder), "zerolatency", TRUE);
    gst_object_unref(encoder);

    GstCaps *caps = gst_caps_new_simple("video/x-raw",
        "format", G_TYPE_STRING, "RGBA",
        "width", G_TYPE_INT, s->config.width,
        "height", G_TYPE_INT, s->config.height,
        "framerate", GST_TYPE_FRACTION, s->config.fps, 1,
        "pixel-aspect-ratio", GST_TYPE_FRACTION, 1, 1,
        gpu ? "texture-target" : NULL, G_TYPE_STRING, "2D", NULL);
    if (gpu) gst_caps_set_features(caps, 0,
        gst_caps_features_new(GST_CAPS_FEATURE_MEMORY_GL_MEMORY, NULL));
    gst_app_src_set_caps(GST_APP_SRC(b->source), caps);
    gst_caps_unref(caps);
    g_object_set(b->source, "format", GST_FORMAT_TIME,
        "block", s->config.output == MC_OUTPUT_MP4,
        "max-buffers", (guint64)4,
        "max-bytes", (guint64)0,
        "leaky-type", s->config.output == MC_OUTPUT_MP4
            ? GST_APP_LEAKY_TYPE_NONE : GST_APP_LEAKY_TYPE_DOWNSTREAM,
        NULL);
    g_signal_connect(b->sink, "new-sample", G_CALLBACK(encoded_sample), b);
    b->bus = gst_element_get_bus(b->pipeline);
    b->encoder_name = g_strdup(name);
    GstStateChangeReturn state = gst_element_set_state(b->pipeline, GST_STATE_PLAYING);
    if (state == GST_STATE_CHANGE_FAILURE) {
        mc_set_error(s, "Could not start GStreamer hardware encoder %s", name);
        return FALSE;
    }
    b->encoder_started = TRUE;
    b->gpu_mode = gpu;
    b->software_mode = software;
    return TRUE;
}

static void clear_encoder(Backend *b) {
    if (b->pipeline) gst_element_set_state(b->pipeline, GST_STATE_NULL);
    if (b->source) gst_object_unref(b->source);
    if (b->sink) gst_object_unref(b->sink);
    if (b->bus) gst_object_unref(b->bus);
    if (b->pipeline) gst_object_unref(b->pipeline);
    b->source = b->sink = b->pipeline = NULL;
    b->bus = NULL;
    g_clear_pointer(&b->encoder_name, g_free);
    b->encoder_started = FALSE;
    b->gpu_mode = FALSE;
    b->software_mode = FALSE;
}

// Pull asynchronous encoder failures back onto the C# render thread.
static gboolean check_encoder_error(Backend *b, MCStream *s) {
    if (g_atomic_int_get(&b->callback_error) < 0) {
        mc_set_error(s, "GStreamer encoded output failed (%s)",
            b->encoder_name ? b->encoder_name : "unknown");
        return FALSE;
    }
    if (b->bus) {
        GstMessage *m = gst_bus_pop_filtered(b->bus, GST_MESSAGE_ERROR);
        if (m) {
            GError *err = NULL;
            gchar *debug = NULL;
            gst_message_parse_error(m, &err, &debug);
            mc_set_error(s, "Hardware encoder %s: %s",
                b->encoder_name ? b->encoder_name : "unknown",
                err ? err->message : "unknown error");
            if (err) g_error_free(err);
            g_free(debug);
            gst_message_unref(m);
            return FALSE;
        }
    }
    return TRUE;
}

// GPU-native fast path: own 4 GPU scratch textures so Raylib's source can be
// reused immediately after the framebuffer blit. Buffer lifetime controls slots.
// No glReadPixels() or system-memory pixel mapping on this route.
static void release_gpu_slot(gpointer data) {
    GPUSlot *slot = data;
    g_atomic_int_set(&slot->busy, 0);
}

static void release_gpu_resources(Backend *b) {
    if (b->context && platform_gl_current(b) == b->context) {
        for (guint i = 0; i < MC_GL_SLOTS; ++i) {
            GPUSlot *slot = &b->slots[i];
            if (slot->framebuffer)
                b->gl.DeleteFramebuffers(1, &slot->framebuffer);
            if (slot->texture)
                b->gl.DeleteTextures(1, &slot->texture);
            slot->framebuffer = slot->texture = 0;
            g_atomic_int_set(&slot->busy, 0);
        }
    }
    if (b->gl_allocator) gst_object_unref(b->gl_allocator);
    if (b->gl_context) gst_object_unref(b->gl_context);
    if (b->gl_display) gst_object_unref(b->gl_display);
    b->gl_allocator = NULL;
    b->gl_context = NULL;
    b->gl_display = NULL;
}

static gboolean initialize_gpu_resources(Backend *b, MCStream *s) {
    b->gl_display = gst_gl_display_new();
    if (!b->gl_display) return FALSE;
#ifdef _WIN32
    GstGLPlatform platform = GST_GL_PLATFORM_WGL;
#else
    GstGLPlatform platform = b->using_egl
        ? GST_GL_PLATFORM_EGL : GST_GL_PLATFORM_GLX;
#endif
    b->gl_context = gst_gl_context_new_wrapped(b->gl_display,
        (guintptr)b->context, platform,
        GST_GL_API_OPENGL | GST_GL_API_OPENGL3);
    if (!b->gl_context) return FALSE;
    GError *error = NULL;
    // The Raylib context is already current on this thread.
    if (!gst_gl_context_activate(b->gl_context, TRUE) ||
        !gst_gl_context_fill_info(b->gl_context, &error)) {
        g_printerr("[Meatcorps.VideoStreaming] GstGLContext: %s\n",
                   error ? error->message : "could not wrap OpenGL context");
        if (error) g_error_free(error);
        return FALSE;
    }
    // Keep Raylib's context bound. Its lifetime is owned by the application.
    b->gl_allocator = gst_gl_memory_allocator_get_default(b->gl_context);
    if (!b->gl_allocator ||
        !gst_video_info_set_format(&b->video_info, GST_VIDEO_FORMAT_RGBA,
                                   s->config.width, s->config.height)) return FALSE;
    MCGLint old_tex = 0, old_read = 0, old_draw = 0;
    b->gl.GetIntegerv(MC_GL_TEXTURE_BINDING_2D, &old_tex);
    b->gl.GetIntegerv(MC_GL_READ_FRAMEBUFFER_BINDING, &old_read);
    b->gl.GetIntegerv(MC_GL_DRAW_FRAMEBUFFER_BINDING, &old_draw);
    gboolean ok = TRUE;
    for (guint i = 0; i < MC_GL_SLOTS; ++i) {
        GPUSlot *slot = &b->slots[i];
        b->gl.GenTextures(1, &slot->texture);
        b->gl.BindTexture(MC_GL_TEXTURE_2D, slot->texture);
        b->gl.TexParameteri(MC_GL_TEXTURE_2D, MC_GL_TEXTURE_MIN_FILTER, MC_GL_NEAREST);
        b->gl.TexParameteri(MC_GL_TEXTURE_2D, MC_GL_TEXTURE_MAG_FILTER, MC_GL_NEAREST);
        b->gl.TexImage2D(MC_GL_TEXTURE_2D, 0, MC_GL_RGBA8,
                         s->config.width, s->config.height, 0,
                         MC_GL_RGBA, MC_GL_UNSIGNED_BYTE, NULL);
        b->gl.GenFramebuffers(1, &slot->framebuffer);
        b->gl.BindFramebuffer(MC_GL_DRAW_FRAMEBUFFER, slot->framebuffer);
        b->gl.FramebufferTexture2D(MC_GL_DRAW_FRAMEBUFFER,
            MC_GL_COLOR_ATTACHMENT0, MC_GL_TEXTURE_2D, slot->texture, 0);
        if (b->gl.CheckFramebufferStatus(MC_GL_DRAW_FRAMEBUFFER) !=
            MC_GL_FRAMEBUFFER_COMPLETE) ok = FALSE;
    }
    b->gl.BindTexture(MC_GL_TEXTURE_2D, (MCGLuint)old_tex);
    b->gl.BindFramebuffer(MC_GL_READ_FRAMEBUFFER, (MCGLuint)old_read);
    b->gl.BindFramebuffer(MC_GL_DRAW_FRAMEBUFFER, (MCGLuint)old_draw);
    return ok && b->gl.GetError() == MC_GL_NO_ERROR;
}

static GstBuffer *capture_gpu_texture(Backend *b, MCStream *s,
                                       uint32_t texture_id, int32_t flip_y) {
    GPUSlot *slot = NULL;
    for (guint i = 0; i < MC_GL_SLOTS; ++i) {
        if (g_atomic_int_compare_and_exchange(&b->slots[i].busy, 0, 1)) {
            slot = &b->slots[i];
            break;
        }
    }
    if (!slot) return NULL;
    MCGLint old_read = 0, old_draw = 0;
    b->gl.GetIntegerv(MC_GL_READ_FRAMEBUFFER_BINDING, &old_read);
    b->gl.GetIntegerv(MC_GL_DRAW_FRAMEBUFFER_BINDING, &old_draw);
    b->gl.BindFramebuffer(MC_GL_READ_FRAMEBUFFER, b->framebuffer);
    b->gl.FramebufferTexture2D(MC_GL_READ_FRAMEBUFFER,
        MC_GL_COLOR_ATTACHMENT0, MC_GL_TEXTURE_2D, texture_id, 0);
    gboolean ok = b->gl.CheckFramebufferStatus(MC_GL_READ_FRAMEBUFFER) ==
                  MC_GL_FRAMEBUFFER_COMPLETE;
    if (ok) {
        b->gl.BindFramebuffer(MC_GL_DRAW_FRAMEBUFFER, slot->framebuffer);
        b->gl.BlitFramebuffer(0, 0, s->config.width, s->config.height,
           0, flip_y ? s->config.height : 0,
           s->config.width, flip_y ? 0 : s->config.height,
           MC_GL_COLOR_BUFFER_BIT, MC_GL_NEAREST);
        b->gl.Finish(); // initial correctness; replace with sync fences later
    }
    MCGLenum error = b->gl.GetError();
    b->gl.BindFramebuffer(MC_GL_READ_FRAMEBUFFER, (MCGLuint)old_read);
    b->gl.BindFramebuffer(MC_GL_DRAW_FRAMEBUFFER, (MCGLuint)old_draw);
    if (!ok || error != MC_GL_NO_ERROR) {
        g_atomic_int_set(&slot->busy, 0);
        mc_set_error(s, "GPU-side framebuffer blit failed (GL 0x%x)", error);
        return NULL;
    }

    GstGLVideoAllocationParams *params =
        gst_gl_video_allocation_params_new_wrapped_texture(
            b->gl_context, NULL, &b->video_info, 0, NULL,
            GST_GL_TEXTURE_TARGET_2D, GST_VIDEO_GL_TEXTURE_TYPE_RGBA,
            slot->texture, slot, release_gpu_slot);
    if (!params) {
        g_atomic_int_set(&slot->busy, 0);
        return NULL;
    }
    GstMemory *mem = (GstMemory *)gst_gl_base_memory_alloc(
        GST_GL_BASE_MEMORY_ALLOCATOR(b->gl_allocator),
        (GstGLAllocationParams *)params);
    gst_gl_allocation_params_free((GstGLAllocationParams *)params);
    if (!mem) {
        g_atomic_int_set(&slot->busy, 0);
        return NULL;
    }
    GstBuffer *buffer = gst_buffer_new();
    if (!buffer) {
        gst_memory_unref(mem);
        return NULL;
    }
    gst_buffer_append_memory(buffer, mem);
    return buffer;
}

static gboolean capture_rgba(Backend *b, MCStream *s,
                             uint32_t texture_id, int32_t flip_y,
                             GstBuffer *buffer) {
    GLFunctions *gl = &b->gl;
    GstMapInfo map;
    if (!gst_buffer_map(buffer, &map, GST_MAP_WRITE)) return FALSE;
    if (map.size < b->row_bytes * (gsize)s->config.height) {
        gst_buffer_unmap(buffer, &map);
        return FALSE;
    }

    MCGLint old_read = 0, old_pack = 4, old_pbo = 0;
    MCGLint old_row_length = 0, old_skip_rows = 0, old_skip_pixels = 0;
    gl->GetIntegerv(MC_GL_READ_FRAMEBUFFER_BINDING, &old_read);
    gl->GetIntegerv(MC_GL_PACK_ALIGNMENT, &old_pack);
    gl->GetIntegerv(MC_GL_PIXEL_PACK_BUFFER_BINDING, &old_pbo);
    gl->GetIntegerv(MC_GL_PACK_ROW_LENGTH, &old_row_length);
    gl->GetIntegerv(MC_GL_PACK_SKIP_ROWS, &old_skip_rows);
    gl->GetIntegerv(MC_GL_PACK_SKIP_PIXELS, &old_skip_pixels);

    gl->BindFramebuffer(MC_GL_READ_FRAMEBUFFER, b->framebuffer);
    gl->FramebufferTexture2D(MC_GL_READ_FRAMEBUFFER, MC_GL_COLOR_ATTACHMENT0,
                             MC_GL_TEXTURE_2D, texture_id, 0);
    gboolean valid = gl->CheckFramebufferStatus(MC_GL_READ_FRAMEBUFFER)
                       == MC_GL_FRAMEBUFFER_COMPLETE;
    if (valid) {
        gl->BindBuffer(MC_GL_PIXEL_PACK_BUFFER, 0);
        gl->PixelStorei(MC_GL_PACK_ALIGNMENT, 1);
        gl->PixelStorei(MC_GL_PACK_ROW_LENGTH, 0);
        gl->PixelStorei(MC_GL_PACK_SKIP_ROWS, 0);
        gl->PixelStorei(MC_GL_PACK_SKIP_PIXELS, 0);
        gl->ReadPixels(0, 0, s->config.width, s->config.height,
                       MC_GL_RGBA, MC_GL_UNSIGNED_BYTE, map.data);

        // OpenGL reads rows bottom-to-top; GStreamer expects top-to-bottom.
        // flip_y toggles the orientation, mirroring the macOS public API.
        if (flip_y) {
            guint8 *data = map.data;
            for (int y = 0; y < s->config.height / 2; ++y) {
                guint8 *top = data + (gsize)y * b->row_bytes;
                guint8 *bottom = data + (gsize)(s->config.height - y - 1) * b->row_bytes;
                memcpy(b->flip_row, top, b->row_bytes);
                memcpy(top, bottom, b->row_bytes);
                memcpy(bottom, b->flip_row, b->row_bytes);
            }
        }
    }
    MCGLenum error = gl->GetError();
    gl->BindBuffer(MC_GL_PIXEL_PACK_BUFFER, (MCGLuint)old_pbo);
    gl->PixelStorei(MC_GL_PACK_ALIGNMENT, old_pack);
    gl->PixelStorei(MC_GL_PACK_ROW_LENGTH, old_row_length);
    gl->PixelStorei(MC_GL_PACK_SKIP_ROWS, old_skip_rows);
    gl->PixelStorei(MC_GL_PACK_SKIP_PIXELS, old_skip_pixels);
    gl->BindFramebuffer(MC_GL_READ_FRAMEBUFFER, (MCGLuint)old_read);
    gst_buffer_unmap(buffer, &map);
    if (!valid || error != MC_GL_NO_ERROR) {
        mc_set_error(s, "GL framebuffer readback failed (FBO=%d, error=0x%x)",
                     (int)valid, (unsigned)error);
        return FALSE;
    }
    return TRUE;
}

static gboolean start_compatibility_encoder(Backend *b, MCStream *s,
                                            const char *why,
                                            gboolean software_only) {
    gboolean selected = FALSE;
    // Readback + installed hardware encoder, then software fallback.
        MCEncoder preferred = s->config.encoder == MC_ENCODER_AUTO
            ? preferred_for_gpu(b) : s->config.encoder;
        for (int pass = 0; pass < 2 && !selected && !software_only; ++pass) {
            for (guint i = 0; i < G_N_ELEMENTS(encoders); ++i) {
                if (s->config.encoder != MC_ENCODER_AUTO &&
                    s->config.encoder != encoders[i].choice) continue;
                if (preferred != MC_ENCODER_AUTO &&
                    ((pass == 0) != (encoders[i].choice == preferred))) continue;
                if (preferred == MC_ENCODER_AUTO && pass == 1) continue;
                const char *name = s->config.codec == MC_CODEC_H265
                    ? encoders[i].h265 : encoders[i].h264;
                if (!factory_exists(name)) continue;
                if (setup_encoder(b, s, name, FALSE)) {
                    warn_fallback("Windows", why, name);
                    selected = TRUE;
                    break;
                }
                clear_encoder(b);
            }
        }
    for (guint i = 0; !selected; ++i) {
        const char *name = software_codec_name(s->config.codec, i);
        if (!name) break;
        if (!factory_exists(name)) continue;
        if (setup_encoder(b, s, name, FALSE)) {
            warn_fallback("Windows", why, name);
            selected = TRUE;
            break;
        }
        clear_encoder(b);
    }
    if (!selected) {
        mc_set_error(s, "No usable GStreamer H.264/H.265 encoder (including software fallback)");
    }
    return selected;
}

MCResult mc_backend_start(MCStream *s) {
    // An unavailable requested encoder is a warning/fallback, not failure.
    if (s->config.width > G_MAXINT / 4 || s->config.height > G_MAXINT ||
        (gsize)s->config.width > G_MAXSIZE / (gsize)s->config.height / 4u) {
        mc_set_error(s, "Video dimensions too large for RGBA capture");
        return MC_ERR_ARGUMENT;
    }
    GError *gst_error = NULL;
    if (!gst_init_check(NULL, NULL, &gst_error)) {
        mc_set_error(s, "GStreamer initialization failed: %s",
            gst_error ? gst_error->message : "unknown");
        if (gst_error) g_error_free(gst_error);
        return MC_ERR_GSTREAMER;
    }

    Backend *b = g_new0(Backend, 1);
    b->stream = s;
    b->row_bytes = (gsize)s->config.width * 4u;
    s->backend = b;
    MCResult result = MC_ERR_GL;
    if (!platform_gl_open(b, s) || !load_gl(b, s)) goto fail;
    b->gl.GenFramebuffers(1, &b->framebuffer);
    if (!b->framebuffer) {
        mc_set_error(s, "OpenGL could not allocate a framebuffer");
        goto fail;
    }
    b->flip_row = g_malloc(b->row_bytes);

    result = MC_ERR_ENCODER;
    gboolean selected = FALSE;
    // Fast GPU path: only NVENC advertises GLMemory RGBA input.
    // Explicit VAAPI/AMF/QSV requests skip this path and try compatibility.
    if (((s->config.encoder == MC_ENCODER_AUTO &&
          preferred_for_gpu(b) == MC_ENCODER_NVENC) ||
         s->config.encoder == MC_ENCODER_NVENC) &&
        factory_exists(s->config.codec == MC_CODEC_H265 ? "nvh265enc" : "nvh264enc")) {
        if (initialize_gpu_resources(b, s)) {
            const char *name = s->config.codec == MC_CODEC_H265 ? "nvh265enc" : "nvh264enc";
            if (setup_encoder(b, s, name, TRUE)) {
                g_print("[Meatcorps.VideoStreaming][Windows] GPU-native GLMemory -> %s (no CPU readback)\n", name);
                selected = TRUE;
            } else clear_encoder(b);
        }
        if (!selected) release_gpu_resources(b);
    }
    if (!selected) selected = start_compatibility_encoder(
        b, s, "GPU-resident hardware path unavailable", FALSE);
    if (!selected) goto fail;
    result = MC_OK;

    result = mc_output_start(s);
    if (result != MC_OK) goto fail;
    b->output_started = TRUE;
    s->clock_start_us = g_get_monotonic_time();
    b->next_frame_us = s->clock_start_us;
    return MC_OK;

fail:
    // Preserve the original error before cleaning partially-started objects.
    char reason[sizeof(s->error)];
    memcpy(reason, s->error, sizeof(reason));
    mc_backend_stop(s);
    memcpy(s->error, reason, sizeof(reason));
    return result;
}

MCResult mc_backend_submit(MCStream *s, uint32_t texture_id, int32_t flip_y) {
    Backend *b = s->backend;
    if (!b || platform_gl_current(b) != b->context) {
        mc_set_error(s, "SubmitTexture requires the original OpenGL context");
        return MC_ERR_GL;
    }
    if (!check_encoder_error(b, s)) {
        // Gst may report a device/negotiation error only after frame 1.
        // Recover into the CPU-visible path once instead of killing streaming.
        if (!b->software_mode) {
            gboolean was_gpu = b->gpu_mode;
            g_printerr("[Meatcorps.VideoStreaming][WARNING] %s encoder failed at runtime. "
                       "Switching to compatibility fallback.\n",
                       was_gpu ? "GPU-native" : "hardware");
            clear_encoder(b);
            if (was_gpu) release_gpu_resources(b);
            g_atomic_int_set(&b->callback_error, 0);
            if (start_compatibility_encoder(b, s,
                    was_gpu ? "GPU-native encoder failed at runtime" :
                              "hardware encoder failed at runtime",
                    !was_gpu)) {
                return MC_FRAME_DROPPED; // one frame lost at transition
            }
        }
        return MC_ERR_ENCODER;
    }
    gint64 now = g_get_monotonic_time();
    if (now < b->next_frame_us) return MC_FRAME_SKIPPED;
    b->next_frame_us = now + 1000000 / s->config.fps;

    GstBuffer *raw = NULL;
    if (b->gpu_mode) {
        raw = capture_gpu_texture(b, s, texture_id, flip_y);
        if (!raw) return MC_FRAME_DROPPED; // Busy GPU slots; don't stall.
    } else {
        gsize total_size = b->row_bytes * (gsize)s->config.height;
        raw = gst_buffer_new_allocate(NULL, total_size, NULL);
        if (!raw) {
            mc_set_error(s, "Failed to allocate raw frame");
            return MC_ERR_GSTREAMER;
        }
        if (!capture_rgba(b, s, texture_id, flip_y, raw)) {
            gst_buffer_unref(raw);
            return MC_ERR_GL;
        }
    }
    GST_BUFFER_PTS(raw) = (GstClockTime)(now - s->clock_start_us) * 1000u;
    GST_BUFFER_DTS(raw) = GST_BUFFER_PTS(raw);
    GST_BUFFER_DURATION(raw) = gst_util_uint64_scale(1, GST_SECOND, s->config.fps);

    GstFlowReturn flow = gst_app_src_push_buffer(GST_APP_SRC(b->source), raw);
    if (flow == GST_FLOW_FLUSHING || flow == GST_FLOW_EOS) return MC_FRAME_DROPPED;
    if (flow != GST_FLOW_OK) {
        mc_set_error(s, "Raw frame submission failed: %d", (int)flow);
        return MC_ERR_GSTREAMER;
    }
    return MC_OK;
}

MCResult mc_backend_stop(MCStream *s) {
    Backend *b = s->backend;
    if (!b) {
        if (s->output) mc_output_destroy(s);
        return MC_OK;
    }
    MCResult result = MC_OK;
    if (b->encoder_started && b->output_started && b->source && b->bus) {
        (void)gst_app_src_end_of_stream(GST_APP_SRC(b->source));
        GstMessage *message = gst_bus_timed_pop_filtered(b->bus,
            5 * GST_SECOND, GST_MESSAGE_EOS | GST_MESSAGE_ERROR);
        if (message) {
            if (GST_MESSAGE_TYPE(message) == GST_MESSAGE_ERROR) {
                GError *err = NULL;
                gchar *detail = NULL;
                gst_message_parse_error(message, &err, &detail);
                mc_set_error(s, "Encoder drain failed: %s",
                    err ? err->message : "unknown");
                if (err) g_error_free(err);
                g_free(detail);
                result = MC_ERR_ENCODER;
            }
            gst_message_unref(message);
        } else {
            mc_set_error(s, "Hardware encoder drain timed out");
            result = MC_ERR_ENCODER;
        }
    }
    // Encoded callbacks have completed before MP4's EOS/finalization.
    clear_encoder(b);
    release_gpu_resources(b);
    if (s->output) {
        if (b->output_started && mc_output_stop(s) != MC_OK)
            result = MC_ERR_GSTREAMER;
        mc_output_destroy(s);
    }

    if (b->framebuffer && b->gl.DeleteFramebuffers) {
        if (platform_gl_current(b) == b->context)
            b->gl.DeleteFramebuffers(1, &b->framebuffer);
        else {
            mc_set_error(s, "Stop requires the original OpenGL context to release its FBO");
            result = MC_ERR_GL;
        }
    }
    platform_gl_close(b);
    g_free(b->flip_row);
    g_free(b);
    s->backend = NULL;
    return result;
}

void mc_backend_destroy(MCStream *s) { (void)s; }

// ---- Operating-system OpenGL loader ----

// OpenGL 3.x entry points aren't exported directly by opengl32.dll.
// Raylib's current WGL context supplies the extension functions.
static void *windows_symbol(HMODULE library, const char *name) {
    return library ? (void *)GetProcAddress(library, name) : NULL;
}

static void *windows_current_context(Backend *b) {
    return b->wgl_get_current ? (void *)b->wgl_get_current() : NULL;
}

static gboolean platform_gl_open(Backend *b, MCStream *s) {
    b->gl_library = LoadLibraryA("opengl32.dll");
    if (!b->gl_library) {
        mc_set_error(s, "Could not load opengl32.dll");
        return FALSE;
    }
    b->wgl_get_current = (HGLRC (WINAPI *)(void))
        GetProcAddress(b->gl_library, "wglGetCurrentContext");
    b->wgl_get_proc = (PROC (WINAPI *)(LPCSTR))
        GetProcAddress(b->gl_library, "wglGetProcAddress");
    b->context = windows_current_context(b);
    if (!b->context) {
        mc_set_error(s, "Start requires a current Raylib WGL context");
        return FALSE;
    }
    return TRUE;
}

static void *platform_gl_proc(Backend *b, const char *name) {
    void *sym = b->wgl_get_proc ? (void *)b->wgl_get_proc(name) : NULL;
    // Windows reports these sentinel values for unavailable WGL symbols.
    if (sym == NULL || sym == (void *)1 || sym == (void *)2 ||
        sym == (void *)3 || sym == (void *)(intptr_t)-1)
        sym = windows_symbol(b->gl_library, name);
    return sym;
}

static void platform_gl_close(Backend *b) {
    if (b->gl_library) FreeLibrary(b->gl_library);
    b->gl_library = NULL;
}

static void *platform_gl_current(Backend *b) { return windows_current_context(b); }
