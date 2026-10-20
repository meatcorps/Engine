#include "backend.h"
#include "common.h"

#include <CoreFoundation/CoreFoundation.h>
#include <CoreMedia/CoreMedia.h>
#include <CoreVideo/CoreVideo.h>
#include <CoreVideo/CVPixelBufferIOSurface.h>
#include <IOSurface/IOSurface.h>
#include <VideoToolbox/VideoToolbox.h>
#include <OpenGL/OpenGL.h>
#include <OpenGL/CGLIOSurface.h>
#include <OpenGL/gl3.h>
#include <glib.h>

#define MC_SLOT_COUNT 4

typedef struct {
    CVPixelBufferRef pixel;
    GLuint texture;
    GLuint framebuffer;
    gboolean busy;
} FrameSlot;

typedef struct {
    MCStream *stream;
    CGLContextObj gl_context;
    GLuint source_framebuffer;
    VTCompressionSessionRef encoder;
    FrameSlot slots[MC_SLOT_COUNT];
    GMutex slot_mutex;
    gint callback_error; // atomically written by VideoToolbox thread
    gint64 first_us;
    gint64 next_us;
} MacBackend;

// Both H.264 and HEVC from VideoToolbox use length-prefixed NAL units.
// Convert to Annex B with SPS/PPS (and VPS for HEVC) on keyframes.
// This only copies COMPRESSED data, never the 1080p RGBA image.
static OSStatus parameter_set(CMFormatDescriptionRef fmt, MCCodec codec,
                              size_t index, const uint8_t **data,
                              size_t *size, size_t *count, int *length) {
    if (codec == MC_CODEC_H265)
        return CMVideoFormatDescriptionGetHEVCParameterSetAtIndex(
            fmt, index, data, size, count, length);
    return CMVideoFormatDescriptionGetH264ParameterSetAtIndex(
        fmt, index, data, size, count, length);
}

static GstBuffer *to_annexb(CMSampleBufferRef sample, MCCodec codec,
                            gboolean *is_keyframe) {
    CFArrayRef attachments = CMSampleBufferGetSampleAttachmentsArray(sample, false);
    *is_keyframe = TRUE;
    if (attachments && CFArrayGetCount(attachments) > 0) {
        CFDictionaryRef dict = CFArrayGetValueAtIndex(attachments, 0);
        CFBooleanRef not_sync = CFDictionaryGetValue(dict, kCMSampleAttachmentKey_NotSync);
        *is_keyframe = (not_sync != kCFBooleanTrue);
    }

    CMBlockBufferRef data_block = CMSampleBufferGetDataBuffer(sample);
    if (!data_block) return NULL;
    size_t size = (size_t)CMBlockBufferGetDataLength(data_block);
    if (!size) return NULL;

    uint8_t *avcc = g_malloc(size);
    if (CMBlockBufferCopyDataBytes(data_block, 0, size, avcc) != kCMBlockBufferNoErr) {
        g_free(avcc);
        return NULL;
    }
    GByteArray *bytes = g_byte_array_sized_new((guint)(size + 256));
    static const guint8 start_code[4] = { 0, 0, 0, 1 };
    int nal_length_size = 4;
    CMFormatDescriptionRef fmt = CMSampleBufferGetFormatDescription(sample);
    size_t count = 0, parameter_size = 0;
    const uint8_t *parameter = NULL;
    if (!fmt || parameter_set(fmt, codec, 0, &parameter, &parameter_size,
                              &count, &nal_length_size) != noErr) {
        g_byte_array_unref(bytes);
        g_free(avcc);
        return NULL;
    }
    if (*is_keyframe) {
        for (size_t i = 0; i < count; ++i) {
            if (parameter_set(fmt, codec, i, &parameter, &parameter_size,
                              NULL, NULL) != noErr || !parameter) {
                g_byte_array_unref(bytes);
                g_free(avcc);
                return NULL;
            }
            g_byte_array_append(bytes, start_code, 4);
            g_byte_array_append(bytes, parameter, (guint)parameter_size);
        }
    }

    if (nal_length_size < 1 || nal_length_size > 4) {
        g_byte_array_unref(bytes);
        g_free(avcc);
        return NULL;
    }
    size_t pos = 0;
    while (pos + (size_t)nal_length_size <= size) {
        uint32_t nal_size = 0;
        for (int i = 0; i < nal_length_size; ++i)
            nal_size = (nal_size << 8) | avcc[pos++];
        if (nal_size == 0 || nal_size > size - pos) break;
        g_byte_array_append(bytes, start_code, 4);
        g_byte_array_append(bytes, avcc + pos, nal_size);
        pos += nal_size;
    }
    g_free(avcc);
    if (pos != size || bytes->len == 0) {
        g_byte_array_unref(bytes);
        return NULL;
    }

    GstBuffer *out = gst_buffer_new_allocate(NULL, bytes->len, NULL);
    if (out) gst_buffer_fill(out, 0, bytes->data, bytes->len);
    g_byte_array_unref(bytes);
    return out;
}

// This callback can run on another thread. It must never call OpenGL.
static void on_encoded(void *context, void *frame_context, OSStatus status,
                       VTEncodeInfoFlags flags, CMSampleBufferRef sample) {
    (void)flags;
    MacBackend *b = context;
    FrameSlot *slot = frame_context;

    if (status == noErr && sample && CMSampleBufferDataIsReady(sample)) {
        gboolean keyframe = FALSE;
        GstBuffer *buffer = to_annexb(sample, b->stream->config.codec, &keyframe);
        if (buffer) {
            CMTime pts = CMSampleBufferGetPresentationTimeStamp(sample);
            if (CMTIME_IS_NUMERIC(pts) && pts.value >= 0 && pts.timescale > 0) {
                GST_BUFFER_PTS(buffer) = gst_util_uint64_scale(
                    (guint64)pts.value, GST_SECOND, (guint64)pts.timescale);
                GST_BUFFER_DTS(buffer) = GST_BUFFER_PTS(buffer); // B-frames are disabled
            }
            GST_BUFFER_DURATION(buffer) = gst_util_uint64_scale(
                GST_SECOND, 1, (guint64)b->stream->config.fps);
            if (!keyframe) GST_BUFFER_FLAG_SET(buffer, GST_BUFFER_FLAG_DELTA_UNIT);
            if (mc_output_push(b->stream, buffer) != MC_OK)
                g_atomic_int_set(&b->callback_error, MC_ERR_GSTREAMER);
        } else {
            g_atomic_int_set(&b->callback_error, MC_ERR_ENCODER);
        }
    } else {
        g_atomic_int_set(&b->callback_error, MC_ERR_ENCODER);
    }
    // The encoder no longer needs this frame. It may be reused by the render thread.
    g_mutex_lock(&b->slot_mutex);
    if (slot) slot->busy = FALSE;
    g_mutex_unlock(&b->slot_mutex);
}

static MCResult create_slot(MacBackend *b, FrameSlot *slot) {
    MCStream *s = b->stream;
    CFMutableDictionaryRef attrs = CFDictionaryCreateMutable(kCFAllocatorDefault, 3,
        &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);
    CFDictionaryRef empty = CFDictionaryCreate(kCFAllocatorDefault, NULL, NULL, 0,
        &kCFTypeDictionaryKeyCallBacks, &kCFTypeDictionaryValueCallBacks);
    CFDictionarySetValue(attrs, kCVPixelBufferIOSurfacePropertiesKey, empty);
    CFDictionarySetValue(attrs, kCVPixelBufferOpenGLCompatibilityKey, kCFBooleanTrue);
    CFDictionarySetValue(attrs, kCVPixelBufferIOSurfaceOpenGLTextureCompatibilityKey, kCFBooleanTrue);
    CFDictionarySetValue(attrs, kCVPixelBufferIOSurfaceOpenGLFBOCompatibilityKey, kCFBooleanTrue);
    CFRelease(empty);

    CVReturn cv = CVPixelBufferCreate(kCFAllocatorDefault, s->config.width,
        s->config.height, kCVPixelFormatType_32BGRA, attrs, &slot->pixel);
    CFRelease(attrs);
    if (cv != kCVReturnSuccess || !slot->pixel) {
        mc_set_error(s, "CVPixelBufferCreate failed: %d", cv);
        return MC_ERR_GL;
    }
    IOSurfaceRef surface = CVPixelBufferGetIOSurface(slot->pixel);
    if (!surface) {
        mc_set_error(s, "CVPixelBuffer is not IOSurface-backed");
        return MC_ERR_GL;
    }

    glGenTextures(1, &slot->texture);
    glBindTexture(GL_TEXTURE_RECTANGLE, slot->texture);
    glTexParameteri(GL_TEXTURE_RECTANGLE, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_RECTANGLE, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
    CGLError cg = CGLTexImageIOSurface2D(b->gl_context, GL_TEXTURE_RECTANGLE,
        GL_RGBA8, s->config.width, s->config.height,
        GL_BGRA, GL_UNSIGNED_INT_8_8_8_8_REV, surface, 0);
    if (cg != kCGLNoError) {
        mc_set_error(s, "CGLTexImageIOSurface2D failed: %d", (int)cg);
        return MC_ERR_GL;
    }
    glGenFramebuffers(1, &slot->framebuffer);
    glBindFramebuffer(GL_DRAW_FRAMEBUFFER, slot->framebuffer);
    glFramebufferTexture2D(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0,
                           GL_TEXTURE_RECTANGLE, slot->texture, 0);
    if (glCheckFramebufferStatus(GL_DRAW_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE) {
        mc_set_error(s, "IOSurface OpenGL framebuffer incomplete");
        return MC_ERR_GL;
    }
    return MC_OK;
}

MCResult mc_backend_start(MCStream *s) {
    if (s->config.encoder != MC_ENCODER_AUTO &&
        s->config.encoder != MC_ENCODER_VIDEOTOOLBOX) {
        mc_set_error(s, "Selected encoder is unavailable in macOS backend");
        return MC_ERR_PLATFORM;
    }
    CGLContextObj current = CGLGetCurrentContext();
    if (!current) {
        mc_set_error(s, "Raylib OpenGL context must be current during Start");
        return MC_ERR_GL;
    }

    MacBackend *b = g_new0(MacBackend, 1);
    b->stream = s;
    b->gl_context = current;
    g_mutex_init(&b->slot_mutex);
    s->backend = b;

    GLint prev_read = 0, prev_draw = 0, prev_rectangle = 0;
    glGetIntegerv(GL_READ_FRAMEBUFFER_BINDING, &prev_read);
    glGetIntegerv(GL_DRAW_FRAMEBUFFER_BINDING, &prev_draw);
    glGetIntegerv(GL_TEXTURE_BINDING_RECTANGLE, &prev_rectangle);
    MCResult result = MC_OK;
    for (int i = 0; i < MC_SLOT_COUNT; ++i) {
        result = create_slot(b, &b->slots[i]);
        if (result != MC_OK) break;
    }
    glBindFramebuffer(GL_READ_FRAMEBUFFER, (GLuint)prev_read);
    glBindFramebuffer(GL_DRAW_FRAMEBUFFER, (GLuint)prev_draw);
    glBindTexture(GL_TEXTURE_RECTANGLE, (GLuint)prev_rectangle);
    if (result != MC_OK) return result;

    glGenFramebuffers(1, &b->source_framebuffer);
    CFMutableDictionaryRef required_hardware = CFDictionaryCreateMutable(
        kCFAllocatorDefault, 1, &kCFTypeDictionaryKeyCallBacks,
        &kCFTypeDictionaryValueCallBacks);
    CFDictionarySetValue(required_hardware,
        kVTVideoEncoderSpecification_RequireHardwareAcceleratedVideoEncoder,
        kCFBooleanTrue);

    OSStatus os = VTCompressionSessionCreate(kCFAllocatorDefault, s->config.width,
        s->config.height,
        s->config.codec == MC_CODEC_H265 ? kCMVideoCodecType_HEVC
                                         : kCMVideoCodecType_H264,
        required_hardware,
        NULL, NULL, on_encoded, b, &b->encoder);
    CFRelease(required_hardware);
    if (os != noErr || !b->encoder) {
        mc_set_error(s, "Hardware VTCompressionSessionCreate failed: %d", (int)os);
        return MC_ERR_ENCODER;
    }

    // No B frames => output callbacks can be consumed in display order.
    os = VTSessionSetProperty(b->encoder, kVTCompressionPropertyKey_RealTime, kCFBooleanTrue);
    if (os == noErr) os = VTSessionSetProperty(b->encoder,
        kVTCompressionPropertyKey_AllowFrameReordering, kCFBooleanFalse);
    int bitrate = s->config.bitrate_kbps * 1000;
    CFNumberRef br = CFNumberCreate(kCFAllocatorDefault, kCFNumberIntType, &bitrate);
    if (os == noErr) os = VTSessionSetProperty(b->encoder,
        kVTCompressionPropertyKey_AverageBitRate, br);
    CFRelease(br);
    int interval = s->config.fps;
    CFNumberRef kf = CFNumberCreate(kCFAllocatorDefault, kCFNumberIntType, &interval);
    if (os == noErr) os = VTSessionSetProperty(b->encoder,
        kVTCompressionPropertyKey_MaxKeyFrameInterval, kf);
    CFRelease(kf);
    if (os == noErr) os = VTCompressionSessionPrepareToEncodeFrames(b->encoder);
    if (os != noErr) {
        mc_set_error(s, "Configuring VideoToolbox failed: %d", (int)os);
        return MC_ERR_ENCODER;
    }

    result = mc_output_start(s);
    if (result != MC_OK) return result;

    s->clock_start_us = g_get_monotonic_time();

    b->first_us = s->clock_start_us;
    b->next_us = b->first_us;

    return MC_OK;
}

MCResult mc_backend_submit(MCStream *s, uint32_t texture_id, int32_t flip_y) {
    MacBackend *b = s->backend;
    if (!b || CGLGetCurrentContext() != b->gl_context) {
        mc_set_error(s, "SubmitTexture must run on the same current OpenGL context");
        return MC_ERR_GL;
    }
    if (g_atomic_int_get(&b->callback_error) < 0) {
        mc_set_error(s, "VideoToolbox callback or GStreamer output failed");
        return MC_ERR_ENCODER;
    }

    gint64 now = g_get_monotonic_time();
    if (now < b->next_us) return MC_FRAME_SKIPPED;
    b->next_us = now + (1000000 / s->config.fps);

    FrameSlot *slot = NULL;
    g_mutex_lock(&b->slot_mutex);
    for (int i = 0; i < MC_SLOT_COUNT; ++i) {
        if (!b->slots[i].busy) {
            slot = &b->slots[i];
            slot->busy = TRUE;
            break;
        }
    }
    g_mutex_unlock(&b->slot_mutex);
    if (!slot) return MC_FRAME_DROPPED;

    GLint old_read = 0, old_draw = 0;
    glGetIntegerv(GL_READ_FRAMEBUFFER_BINDING, &old_read);
    glGetIntegerv(GL_DRAW_FRAMEBUFFER_BINDING, &old_draw);
    glBindFramebuffer(GL_READ_FRAMEBUFFER, b->source_framebuffer);
    glFramebufferTexture2D(GL_READ_FRAMEBUFFER, GL_COLOR_ATTACHMENT0,
        GL_TEXTURE_2D, texture_id, 0);
    glBindFramebuffer(GL_DRAW_FRAMEBUFFER, slot->framebuffer);

    GLenum src_status = glCheckFramebufferStatus(GL_READ_FRAMEBUFFER);
    GLenum dst_status = glCheckFramebufferStatus(GL_DRAW_FRAMEBUFFER);
    if (src_status == GL_FRAMEBUFFER_COMPLETE && dst_status == GL_FRAMEBUFFER_COMPLETE) {
        const GLint width = s->config.width;
        const GLint height = s->config.height;

        glBlitFramebuffer(
            0, 0,
            width, height,

            0, flip_y ? height : 0,
            width, flip_y ? 0 : height,

            GL_COLOR_BUFFER_BIT,
            GL_NEAREST
        );
        glFinish(); // deliberately synchronous, replace with fences after initial test
    }
    glBindFramebuffer(GL_READ_FRAMEBUFFER, (GLuint)old_read);
    glBindFramebuffer(GL_DRAW_FRAMEBUFFER, (GLuint)old_draw);

    if (src_status != GL_FRAMEBUFFER_COMPLETE ||
        dst_status != GL_FRAMEBUFFER_COMPLETE || glGetError() != GL_NO_ERROR) {
        g_mutex_lock(&b->slot_mutex);
        slot->busy = FALSE;
        g_mutex_unlock(&b->slot_mutex);
        mc_set_error(s, "OpenGL texture-to-IOSurface framebuffer blit failed");
        return MC_ERR_GL;
    }

    CMTime pts = CMTimeMake(now - b->first_us, 1000000);
    CMTime duration = CMTimeMake(1, s->config.fps);
    OSStatus os = VTCompressionSessionEncodeFrame(b->encoder,
        slot->pixel, pts, duration, NULL, slot, NULL);
    if (os != noErr) {
        g_mutex_lock(&b->slot_mutex);
        slot->busy = FALSE;
        g_mutex_unlock(&b->slot_mutex);
        mc_set_error(s, "VTCompressionSessionEncodeFrame failed: %d", (int)os);
        return MC_ERR_ENCODER;
    }
    return MC_OK;
}

MCResult mc_backend_stop(MCStream *s) {
    MacBackend *b = s->backend;
    if (!b) return MC_OK;
    MCResult result = MC_OK;
    if (b->encoder) {
        OSStatus os = VTCompressionSessionCompleteFrames(b->encoder, kCMTimeInvalid);
        if (os != noErr) {
            mc_set_error(s, "VTCompressionSessionCompleteFrames failed: %d", (int)os);
            result = MC_ERR_ENCODER;
        }
    }
    // All compression callbacks have completed before we end appsrc.
    if (mc_output_stop(s) != MC_OK) result = MC_ERR_GSTREAMER;
    mc_output_destroy(s);
    if (b->encoder) {
        VTCompressionSessionInvalidate(b->encoder);
        CFRelease(b->encoder);
        b->encoder = NULL;
    }

    // Stop/Destroy must be called with Raylib's original GL context current.
    if (CGLGetCurrentContext() == b->gl_context) {
        if (b->source_framebuffer) glDeleteFramebuffers(1, &b->source_framebuffer);
        for (int i = 0; i < MC_SLOT_COUNT; ++i) {
            if (b->slots[i].framebuffer) glDeleteFramebuffers(1, &b->slots[i].framebuffer);
            if (b->slots[i].texture) glDeleteTextures(1, &b->slots[i].texture);
        }
    } else {
        mc_set_error(s, "Stop was called without the original GL context; GL resources cannot be freed");
        result = MC_ERR_GL;
    }
    for (int i = 0; i < MC_SLOT_COUNT; ++i) {
        if (b->slots[i].pixel) CVPixelBufferRelease(b->slots[i].pixel);
    }
    g_mutex_clear(&b->slot_mutex);
    g_free(b);
    s->backend = NULL;
    return result;
}

void mc_backend_destroy(MCStream *s) { (void)s; }
