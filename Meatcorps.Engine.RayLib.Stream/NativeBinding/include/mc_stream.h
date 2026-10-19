#ifndef MC_STREAM_H
#define MC_STREAM_H
#include <stdint.h>

#ifdef _WIN32
#  ifdef MC_BUILD_SHARED
#    define MC_API __declspec(dllexport)
#  else
#    define MC_API __declspec(dllimport)
#  endif
#else
#  define MC_API __attribute__((visibility("default")))
#endif

#ifdef __cplusplus
extern "C" {
#endif

typedef struct MCStream MCStream;

typedef enum {
    MC_OK = 0,
    MC_FRAME_SKIPPED = 1,
    MC_FRAME_DROPPED = 2,
    MC_ERR_ARGUMENT = -1,
    MC_ERR_STATE = -2,
    MC_ERR_PLATFORM = -3,
    MC_ERR_ENCODER = -4,
    MC_ERR_GSTREAMER = -5,
    MC_ERR_GL = -6
} MCResult;

typedef enum {
    MC_ENCODER_AUTO = 0,
    MC_ENCODER_VIDEOTOOLBOX = 1,
    MC_ENCODER_NVENC = 2,
    MC_ENCODER_VAAPI = 3,
    MC_ENCODER_AMF = 4,
    MC_ENCODER_QSV = 5
} MCEncoder;

typedef enum {
    MC_CODEC_H264 = 0,
    MC_CODEC_H265 = 1
} MCCodec;

typedef enum {
    MC_OUTPUT_RTSP = 0, // Server. Clients connect to this machine.
    MC_OUTPUT_MP4 = 1   // Record locally.
} MCOutput;

typedef struct {
    int32_t width;
    int32_t height;
    int32_t fps;
    int32_t bitrate_kbps;
    MCEncoder encoder;
    MCCodec codec;
    MCOutput output;
    const char *location; // RTSP: mount path (/meatcorps). MP4: filename.
    uint16_t port;        // RTSP server listening port; unused for MP4.
    int32_t audio_sample_rate;    // 0 = audio disabled
    int32_t audio_bitrate_kbps;   // 128 is a good starting point
} MCConfig;

MC_API MCResult mc_stream_create(const MCConfig *config, MCStream **out);
MC_API MCResult mc_stream_start(MCStream *stream); // current Raylib GL context required
MC_API MCResult mc_stream_submit_texture(MCStream *stream, uint32_t texture_id, int32_t flip_y);
MC_API MCResult mc_stream_stop(MCStream *stream); // must be called before GL context dies
MC_API void mc_stream_destroy(MCStream *stream);
MC_API const char *mc_stream_last_error(const MCStream *stream);
// Returns elapsed nanoseconds since the streaming session started.
// Safe to call from the audio capture thread.
MC_API uint64_t mc_stream_time_ns(const MCStream *stream);

// PCM must contain interleaved stereo float32 samples.
// frames = number of stereo pairs, NOT number of floats.
// pts_ns uses mc_stream_time_ns() as its clock.
MC_API MCResult mc_stream_submit_audio_f32(MCStream *stream, const float *samples, uint32_t frames, uint64_t pts_ns);

#ifdef __cplusplus
}
#endif
#endif
