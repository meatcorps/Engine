#include "common.h"
#include "backend.h"
#include <glib.h>
#include <stdarg.h>
#include <stdio.h>
#include <string.h>

void mc_set_error(MCStream *stream, const char *fmt, ...) {
    if (!stream) return;
    va_list args;
    va_start(args, fmt);
    vsnprintf(stream->error, sizeof(stream->error), fmt, args);
    va_end(args);
}

MCResult mc_stream_create(const MCConfig *cfg, MCStream **out) {
    if (!out) return MC_ERR_ARGUMENT;
    *out = NULL;
    if (!cfg || cfg->width <= 0 || cfg->height <= 0 || cfg->fps <= 0 ||
        cfg->fps > 240 || cfg->bitrate_kbps <= 0 || cfg->bitrate_kbps > 1000000 ||
        (cfg->codec != MC_CODEC_H264 && cfg->codec != MC_CODEC_H265) ||
        (cfg->output != MC_OUTPUT_MP4 && cfg->output != MC_OUTPUT_RTSP) ||
        (cfg->encoder < MC_ENCODER_AUTO || cfg->encoder > MC_ENCODER_QSV) ||
        !cfg->location || !cfg->location[0] ||
        (cfg->output == MC_OUTPUT_RTSP &&
         (cfg->port == 0 || cfg->location[0] != '/'))
         || cfg->audio_sample_rate < 0
         || cfg->audio_sample_rate > 96000
         || (cfg->audio_sample_rate > 0 &&
         (cfg->audio_sample_rate < 8000 ||
         cfg->audio_bitrate_kbps < 32 ||
         cfg->audio_bitrate_kbps > 320)))
         return MC_ERR_ARGUMENT;

    MCStream *stream = g_new0(MCStream, 1);
    stream->config = *cfg;
    stream->location_owned = g_strdup(cfg->location);
    stream->config.location = stream->location_owned;
    *out = stream;
    return MC_OK;
}

MCResult mc_stream_start(MCStream *stream) {
    if (!stream) return MC_ERR_ARGUMENT;
    if (stream->started || stream->backend) return MC_ERR_STATE;
    MCResult result = mc_backend_start(stream);
    if (result == MC_OK) stream->started = 1;
    return result;
}

MCResult mc_stream_submit_texture(MCStream *stream, uint32_t texture_id, int32_t flip_y) {
    if (!stream || !texture_id) return MC_ERR_ARGUMENT;

    if (!stream->started) return MC_ERR_STATE;

    return mc_backend_submit(stream, texture_id, flip_y);
}

uint64_t mc_stream_time_ns(const MCStream *stream) {
    if (!stream || stream->clock_start_us <= 0)
        return 0;

    gint64 elapsed =
        g_get_monotonic_time() - stream->clock_start_us;

    return elapsed > 0
        ? (uint64_t)elapsed * 1000ULL
        : 0;
}

MCResult mc_stream_submit_audio_f32(MCStream *stream, const float *samples, uint32_t frames, uint64_t pts_ns) {
    if (!stream || !samples || frames == 0)
        return MC_ERR_ARGUMENT;

    if (!stream->started)
        return MC_ERR_STATE;

    if (stream->config.audio_sample_rate == 0)
        return MC_ERR_STATE;

    // Don't allow arbitrarily large input buffers.
    if (frames > (uint32_t)stream->config.audio_sample_rate)
        return MC_ERR_ARGUMENT;

    return mc_output_push_audio(
        stream, samples, frames, pts_ns
    );
}

MCResult mc_stream_stop(MCStream *stream) {
    if (!stream) return MC_ERR_ARGUMENT;
    if (!stream->started && !stream->backend) return MC_OK;
    MCResult result = mc_backend_stop(stream);
    stream->started = 0;
    return result;
}

void mc_stream_destroy(MCStream *stream) {
    if (!stream) return;
    if (stream->started || stream->backend) (void)mc_stream_stop(stream);
    mc_backend_destroy(stream);
    g_free(stream->location_owned);
    g_free(stream);
}

const char *mc_stream_last_error(const MCStream *stream) {
    return stream ? stream->error : "Null MCStream";
}
