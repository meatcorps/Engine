/*
 * Linux VA-API / EGL DMA-BUF / OpenGL encoder for Meatcorps.VideoStreaming.
 *
 * Design based on the public VA-API/FFmpeg/DMA-BUF interoperability model
 * also employed by OBS, without using OBS source code.
 *
 *  OpenGL Raylib RGBA texture
 *    -> GLSL conversion directly into exported VA NV12 surfaces (Y + UV)
 *    -> FFmpeg VAAPI hardware encoder (H.264 or HEVC)
 *    -> compressed Annex-B GstBuffers -> existing common.c (RTSP / MP4)
 *
 * No pixel data passes through the CPU on this path. GPU synchronization is
 * deliberately conservative (glFinish) until the user can profile it.
 *
 * If VA-API EGL interop cannot start, delegate to the existing compatibility
 * backend, which prints clear hardware/software/readback fallback warnings.
 *
 * Requires: C11, GStreamer, FFmpeg libavcodec/libavutil, libva, EGL, OpenGL,
 * libdrm and the original Linux compatibility backend (backend_compat.c).
 */
#define GL_GLEXT_PROTOTYPES
#include "backend.h"
#include "common.h"
#include <glib.h>
#include <GL/gl.h>
#include <GL/glext.h>
#include <GL/glx.h>
#include <EGL/egl.h>
#include <EGL/eglext.h>
#include <drm_fourcc.h>
#include <va/va.h>
#include <va/va_drmcommon.h>
#include <libavcodec/avcodec.h>
#include <libavutil/opt.h>
#include <libavutil/hwcontext.h>
#include <libavutil/hwcontext_vaapi.h>
#include <libavutil/error.h>
#include <unistd.h>
#include <stdint.h>
#include <string.h>

/* Existing fallback is compiled as backend_compat.c with renamed symbols. */
MCResult mc_linux_compat_start(MCStream *s);
MCResult mc_linux_compat_submit(MCStream *s, uint32_t id, int32_t flip_y);
MCResult mc_linux_compat_stop(MCStream *s);
void mc_linux_compat_destroy(MCStream *s);

typedef enum { MC_LINUX_NONE = 0, MC_LINUX_VAAPI, MC_LINUX_COMPAT } LinuxMode;
/* Installed in s->backend only while the VA-API implementation is active. */
typedef struct {
    MCStream *stream;
    void *raylib_context;
    gboolean raylib_egl;
    EGLDisplay egl_display;
    PFNEGLCREATEIMAGEKHRPROC create_image;
    PFNEGLDESTROYIMAGEKHRPROC destroy_image;
    PFNGLEGLIMAGETARGETTEXTURE2DOESPROC image_texture;
    gboolean supports_modifiers;

    AVBufferRef *hw_device;
    AVBufferRef *hw_frames;
    AVCodecContext *encoder;
    AVPacket *packet;
    VADisplay va_display;

    GLuint program_y, program_uv, vao, framebuffer;
    GLint y_input, y_flip, y_height;
    GLint uv_input, uv_flip, uv_height;
    gint64 next_us;
    LinuxMode mode;
} VALinux;

/* The mode is private to this file. A successful compat start replaces
 * s->backend with the compatibility backend's own allocation. */
static GHashTable *compat_streams;  /* protected by mutex; no GPU objects */
static GMutex compat_mutex;
static gsize compat_once;

static void track_compat(MCStream *s, gboolean add) {
    if (g_once_init_enter(&compat_once)) {
        g_mutex_init(&compat_mutex);
        compat_streams = g_hash_table_new(g_direct_hash, g_direct_equal);
        g_once_init_leave(&compat_once, 1);
    }
    g_mutex_lock(&compat_mutex);
    if (add) g_hash_table_add(compat_streams, s);
    else g_hash_table_remove(compat_streams, s);
    g_mutex_unlock(&compat_mutex);
}
static gboolean is_compat(MCStream *s) {
    if (!compat_once) return FALSE;
    g_mutex_lock(&compat_mutex);
    gboolean result = g_hash_table_contains(compat_streams, s);
    g_mutex_unlock(&compat_mutex);
    return result;
}

static void log_ffmpeg(MCStream *s, const char *what, int err) {
    char message[AV_ERROR_MAX_STRING_SIZE];
    av_strerror(err, message, sizeof(message));
    mc_set_error(s, "%s: %s", what, message);
}

static GLuint shader(GLenum stage, const char *src) {
    GLuint sh = glCreateShader(stage);
    if (!sh) return 0;
    glShaderSource(sh, 1, &src, NULL);
    glCompileShader(sh);
    GLint ok = GL_FALSE;
    glGetShaderiv(sh, GL_COMPILE_STATUS, &ok);
    if (!ok) {
        char log[1024] = {0};
        glGetShaderInfoLog(sh, sizeof(log), NULL, log);
        g_printerr("[Meatcorps.VideoStreaming][Linux] GLSL: %s\n", log);
        glDeleteShader(sh);
        return 0;
    }
    return sh;
}

static GLuint program(const char *fragment) {
    static const char *vertex =
        "#version 330 core\n"
        "void main(){\n"
        "  vec2 p = vec2((gl_VertexID << 1) & 2, gl_VertexID & 2);\n"
        "  gl_Position = vec4(p * 2.0 - 1.0, 0.0, 1.0);\n"
        "}\n";
    GLuint vs = shader(GL_VERTEX_SHADER, vertex);
    GLuint fs = shader(GL_FRAGMENT_SHADER, fragment);
    if (!vs || !fs) {
        if (vs) glDeleteShader(vs);
        if (fs) glDeleteShader(fs);
        return 0;
    }
    GLuint p = glCreateProgram();
    glAttachShader(p, vs);
    glAttachShader(p, fs);
    glLinkProgram(p);
    glDeleteShader(vs);
    glDeleteShader(fs);
    GLint ok = GL_FALSE;
    glGetProgramiv(p, GL_LINK_STATUS, &ok);
    if (!ok) {
        char log[1024] = {0};
        glGetProgramInfoLog(p, sizeof(log), NULL, log);
        g_printerr("[Meatcorps.VideoStreaming][Linux] GLSL link: %s\n", log);
        glDeleteProgram(p);
        return 0;
    }
    return p;
}

/* Limited-range BT.709 NV12: 8-bit Y plane plus interleaved UV plane.
 * Coordinates deliberately match the same flip_y meaning as macOS blit. */
static const char *frag_y =
    "#version 330 core\n"
    "uniform sampler2D texInput; uniform int height; uniform int flipY;\n"
    "layout(location=0) out float outY;\n"
    "void main(){\n"
    " ivec2 p=ivec2(gl_FragCoord.xy);\n"
    " if(flipY!=0) p.y=height-1-p.y;\n"
    " vec3 rgb=texelFetch(texInput,p,0).rgb;\n"
    " outY=(16.0+219.0*dot(rgb,vec3(0.2126,0.7152,0.0722)))/255.0;\n"
    "}\n";
static const char *frag_uv =
    "#version 330 core\n"
    "uniform sampler2D texInput; uniform int height; uniform int flipY;\n"
    "layout(location=0) out vec2 outUV;\n"
    "void main(){\n"
    " ivec2 b=ivec2(gl_FragCoord.xy)*2;\n"
    " vec3 c=vec3(0.0);\n"
    " for(int y=0;y<2;y++) for(int x=0;x<2;x++){\n"
    "  ivec2 p=b+ivec2(x,y);\n"
    "  if(flipY!=0) p.y=height-1-p.y;\n"
    "  c+=texelFetch(texInput,p,0).rgb;\n"
    " }\n"
    " c*=0.25;\n"
    " float l=dot(c,vec3(0.2126,0.7152,0.0722));\n"
    " outUV=(vec2(128.0)+224.0*vec2((c.b-l)/1.8556,(c.r-l)/1.5748))/255.0;\n"
    "}\n";

static void destroy_va(VALinux *b) {
    if (!b) return;
    /* GL resource cleanup needs original GL context. */
    gboolean context_ok = b->raylib_egl
        ? (void *)eglGetCurrentContext() == b->raylib_context
        : (void *)glXGetCurrentContext() == b->raylib_context;
    if (context_ok) {
        if (b->framebuffer) glDeleteFramebuffers(1, &b->framebuffer);
        if (b->vao) glDeleteVertexArrays(1, &b->vao);
        if (b->program_y) glDeleteProgram(b->program_y);
        if (b->program_uv) glDeleteProgram(b->program_uv);
    } else if (b->framebuffer || b->vao || b->program_y || b->program_uv) {
        g_printerr("[Meatcorps.VideoStreaming][Linux][WARNING] "
                   "Stop must use original Raylib GL context; OpenGL objects may leak.\n");
    }
    av_packet_free(&b->packet);
    avcodec_free_context(&b->encoder);
    av_buffer_unref(&b->hw_frames);
    av_buffer_unref(&b->hw_device);
    /* Never terminate an EGL display we didn't create/own. */
    g_free(b);
}

static gboolean va_init(MCStream *s, VALinux *b) {
    /* Prefer current EGL display, otherwise use default display for GLX.
     * On multi-GPU setups MC_VAAPI_DEVICE must match Raylib's GPU. */
    b->raylib_context = (void *)eglGetCurrentContext();
    b->raylib_egl = b->raylib_context != (void *)EGL_NO_CONTEXT;
    if (!b->raylib_egl) b->raylib_context = (void *)glXGetCurrentContext();
    if (!b->raylib_context) {
        mc_set_error(s, "VA-API: Raylib OpenGL context is not current");
        return FALSE;
    }
    b->egl_display = b->raylib_egl ? eglGetCurrentDisplay()
                                    : eglGetDisplay(EGL_DEFAULT_DISPLAY);
    if (b->egl_display == EGL_NO_DISPLAY) {
        mc_set_error(s, "VA-API: EGL display unavailable");
        return FALSE;
    }
    EGLint major = 0, minor = 0;
    if (!eglInitialize(b->egl_display, &major, &minor)) {
        mc_set_error(s, "VA-API: EGL display initialization failed (0x%x)", eglGetError());
        return FALSE;
    }
    const char *ext = eglQueryString(b->egl_display, EGL_EXTENSIONS);
    if (!ext || !strstr(ext, "EGL_EXT_image_dma_buf_import")) {
        mc_set_error(s, "VA-API: EGL_EXT_image_dma_buf_import unavailable");
        return FALSE;
    }
    b->supports_modifiers = strstr(ext, "EGL_EXT_image_dma_buf_import_modifiers") != NULL;
    b->create_image = (PFNEGLCREATEIMAGEKHRPROC)eglGetProcAddress("eglCreateImageKHR");
    b->destroy_image = (PFNEGLDESTROYIMAGEKHRPROC)eglGetProcAddress("eglDestroyImageKHR");
    b->image_texture = (PFNGLEGLIMAGETARGETTEXTURE2DOESPROC)
        eglGetProcAddress("glEGLImageTargetTexture2DOES");
    if (!b->create_image || !b->destroy_image || !b->image_texture) {
        mc_set_error(s, "VA-API: EGL image import entry points unavailable");
        return FALSE;
    }
    /* NV12 uses 2x2 chroma subsampling. */
    if ((s->config.width & 1) || (s->config.height & 1)) {
        mc_set_error(s, "VA-API NV12 requires even width and height");
        return FALSE;
    }
    const char *device = g_getenv("MC_VAAPI_DEVICE");
    if (!device || !*device) device = NULL;
    int err = av_hwdevice_ctx_create(&b->hw_device, AV_HWDEVICE_TYPE_VAAPI,
                                     device, NULL, 0);
    if (err < 0) {
        log_ffmpeg(s, "VA-API device initialization", err);
        return FALSE;
    }
    AVHWDeviceContext *device_context = (AVHWDeviceContext *)b->hw_device->data;
    b->va_display = ((AVVAAPIDeviceContext *)device_context->hwctx)->display;

    b->hw_frames = av_hwframe_ctx_alloc(b->hw_device);
    if (!b->hw_frames) {
        mc_set_error(s, "VA-API hardware frame pool allocation failed");
        return FALSE;
    }
    AVHWFramesContext *frames = (AVHWFramesContext *)b->hw_frames->data;
    frames->format = AV_PIX_FMT_VAAPI;
    frames->sw_format = AV_PIX_FMT_NV12;
    frames->width = s->config.width;
    frames->height = s->config.height;
    frames->initial_pool_size = 12;
    err = av_hwframe_ctx_init(b->hw_frames);
    if (err < 0) {
        log_ffmpeg(s, "VA-API hardware frame pool init", err);
        return FALSE;
    }

    const char *codec_name = s->config.codec == MC_CODEC_H265
        ? "hevc_vaapi" : "h264_vaapi";
    const AVCodec *codec = avcodec_find_encoder_by_name(codec_name);
    if (!codec || !(b->encoder = avcodec_alloc_context3(codec))) {
        mc_set_error(s, "VA-API encoder %s not available in FFmpeg", codec_name);
        return FALSE;
    }
    b->encoder->width = s->config.width;
    b->encoder->height = s->config.height;
    b->encoder->pix_fmt = AV_PIX_FMT_VAAPI;
    b->encoder->time_base = (AVRational){1, 1000000};
    b->encoder->framerate = (AVRational){s->config.fps, 1};
    b->encoder->bit_rate = (int64_t)s->config.bitrate_kbps * 1000;
    b->encoder->gop_size = s->config.fps; // keyframe each second
    b->encoder->max_b_frames = 0;        // low-latency ordering
    b->encoder->hw_frames_ctx = av_buffer_ref(b->hw_frames);
    av_opt_set(b->encoder->priv_data, "rc_mode", "CBR", 0); // best effort
    err = avcodec_open2(b->encoder, codec, NULL);
    if (err < 0) {
        log_ffmpeg(s, "VA-API hardware encoder startup", err);
        return FALSE;
    }
    b->packet = av_packet_alloc();
    if (!b->packet) return FALSE;

    b->program_y = program(frag_y);
    b->program_uv = program(frag_uv);
    if (!b->program_y || !b->program_uv) {
        mc_set_error(s, "VA-API GPU NV12 conversion shader unavailable");
        return FALSE;
    }
    b->y_input = glGetUniformLocation(b->program_y, "texInput");
    b->y_flip = glGetUniformLocation(b->program_y, "flipY");
    b->y_height = glGetUniformLocation(b->program_y, "height");
    b->uv_input = glGetUniformLocation(b->program_uv, "texInput");
    b->uv_flip = glGetUniformLocation(b->program_uv, "flipY");
    b->uv_height = glGetUniformLocation(b->program_uv, "height");
    glGenVertexArrays(1, &b->vao);
    glGenFramebuffers(1, &b->framebuffer);
    if (!b->vao || !b->framebuffer || glGetError() != GL_NO_ERROR) {
        mc_set_error(s, "VA-API OpenGL FBO/VAO creation failed");
        return FALSE;
    }
    return TRUE;
}

/* Creates an imported texture for one VA surface plane. eglCreateImageKHR
 * retains DMA-BUF storage after the exported file descriptors are closed. */
static gboolean import_plane(VALinux *b, const VADRMPRIMESurfaceDescriptor *desc,
                             guint plane, GLuint *tex, EGLImageKHR *image) {
    if (plane >= desc->num_layers) return FALSE;
    if (desc->layers[plane].num_planes != 1) return FALSE;
    if (desc->layers[plane].object_index[0] >= desc->num_objects) return FALSE;
    const uint32_t object_index = desc->layers[plane].object_index[0];
    EGLint attrs[25];
    int k = 0;
    attrs[k++] = EGL_WIDTH;
    attrs[k++] = (EGLint)(plane ? desc->width / 2 : desc->width);
    attrs[k++] = EGL_HEIGHT;
    attrs[k++] = (EGLint)(plane ? desc->height / 2 : desc->height);
    attrs[k++] = EGL_LINUX_DRM_FOURCC_EXT;
    attrs[k++] = (EGLint)desc->layers[plane].drm_format;
    attrs[k++] = EGL_DMA_BUF_PLANE0_FD_EXT;
    attrs[k++] = desc->objects[object_index].fd;
    attrs[k++] = EGL_DMA_BUF_PLANE0_OFFSET_EXT;
    attrs[k++] = (EGLint)desc->layers[plane].offset[0];
    attrs[k++] = EGL_DMA_BUF_PLANE0_PITCH_EXT;
    attrs[k++] = (EGLint)desc->layers[plane].pitch[0];
    if (desc->objects[object_index].drm_format_modifier != DRM_FORMAT_MOD_INVALID &&
        desc->objects[object_index].drm_format_modifier != DRM_FORMAT_MOD_LINEAR) {
        if (!b->supports_modifiers) return FALSE;
        attrs[k++] = EGL_DMA_BUF_PLANE0_MODIFIER_LO_EXT;
        attrs[k++] = (EGLint)(desc->objects[object_index].drm_format_modifier & 0xffffffffu);
        attrs[k++] = EGL_DMA_BUF_PLANE0_MODIFIER_HI_EXT;
        attrs[k++] = (EGLint)(desc->objects[object_index].drm_format_modifier >> 32);
    }
    attrs[k++] = EGL_NONE;
    *image = b->create_image(b->egl_display, EGL_NO_CONTEXT,
                              EGL_LINUX_DMA_BUF_EXT, NULL, attrs);
    if (*image == EGL_NO_IMAGE_KHR) return FALSE;
    glGenTextures(1, tex);
    glBindTexture(GL_TEXTURE_2D, *tex);
    b->image_texture(GL_TEXTURE_2D, (GLeglImageOES)*image);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MIN_FILTER, GL_NEAREST);
    glTexParameteri(GL_TEXTURE_2D, GL_TEXTURE_MAG_FILTER, GL_NEAREST);
    return glGetError() == GL_NO_ERROR;
}

/* Saves rendering state modified by the GLSL NV12 converter. */
typedef struct {
    GLint read_fbo, draw_fbo, program, vao, viewport[4];
    GLint active_tex, texture;
    GLboolean blend, scissor, depth, stencil, cull, dither, srgb;
} SavedGL;
static void save_gl(SavedGL *v) {
    glGetIntegerv(GL_READ_FRAMEBUFFER_BINDING, &v->read_fbo);
    glGetIntegerv(GL_DRAW_FRAMEBUFFER_BINDING, &v->draw_fbo);
    glGetIntegerv(GL_CURRENT_PROGRAM, &v->program);
    glGetIntegerv(GL_VERTEX_ARRAY_BINDING, &v->vao);
    glGetIntegerv(GL_VIEWPORT, v->viewport);
    glGetIntegerv(GL_ACTIVE_TEXTURE, &v->active_tex);
    glActiveTexture(GL_TEXTURE0);
    glGetIntegerv(GL_TEXTURE_BINDING_2D, &v->texture);
    v->blend=glIsEnabled(GL_BLEND); v->scissor=glIsEnabled(GL_SCISSOR_TEST);
    v->depth=glIsEnabled(GL_DEPTH_TEST); v->stencil=glIsEnabled(GL_STENCIL_TEST);
    v->cull=glIsEnabled(GL_CULL_FACE); v->dither=glIsEnabled(GL_DITHER);
    v->srgb=glIsEnabled(GL_FRAMEBUFFER_SRGB);
}
static void restore_gl(const SavedGL *v) {
    glBindFramebuffer(GL_READ_FRAMEBUFFER, (GLuint)v->read_fbo);
    glBindFramebuffer(GL_DRAW_FRAMEBUFFER, (GLuint)v->draw_fbo);
    glUseProgram((GLuint)v->program);
    glBindVertexArray((GLuint)v->vao);
    glViewport(v->viewport[0],v->viewport[1],v->viewport[2],v->viewport[3]);
    glBindTexture(GL_TEXTURE_2D, (GLuint)v->texture);
    glActiveTexture((GLenum)v->active_tex);
#define RESTORE(cap, on) do { if (on) glEnable(cap); else glDisable(cap); } while (0)
    RESTORE(GL_BLEND,v->blend); RESTORE(GL_SCISSOR_TEST,v->scissor);
    RESTORE(GL_DEPTH_TEST,v->depth); RESTORE(GL_STENCIL_TEST,v->stencil);
    RESTORE(GL_CULL_FACE,v->cull); RESTORE(GL_DITHER,v->dither);
    RESTORE(GL_FRAMEBUFFER_SRGB,v->srgb);
#undef RESTORE
}

static gboolean upload_nv12_gpu(VALinux *b, const VADRMPRIMESurfaceDescriptor *desc,
                                GLuint source, gboolean flip) {
    if (desc->num_layers != 2 || desc->layers[0].drm_format != DRM_FORMAT_R8 ||
        desc->layers[1].drm_format != DRM_FORMAT_GR88) return FALSE;
    EGLImageKHR images[2] = {EGL_NO_IMAGE_KHR, EGL_NO_IMAGE_KHR};
    GLuint planes[2] = {0,0};
    SavedGL saved;
    save_gl(&saved);
    gboolean ok = TRUE;
    glDisable(GL_BLEND); glDisable(GL_SCISSOR_TEST);
    glDisable(GL_DEPTH_TEST); glDisable(GL_STENCIL_TEST);
    glDisable(GL_CULL_FACE); glDisable(GL_DITHER);
    glDisable(GL_FRAMEBUFFER_SRGB);
    glBindVertexArray(b->vao);
    glBindFramebuffer(GL_DRAW_FRAMEBUFFER, b->framebuffer);

    for (guint i=0; i<2 && ok; ++i) {
        glActiveTexture(GL_TEXTURE0);
        if (!import_plane(b, desc, i, &planes[i], &images[i])) {
            ok = FALSE;
            break;
        }
        glFramebufferTexture2D(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0,
                               GL_TEXTURE_2D, planes[i], 0);
        if (glCheckFramebufferStatus(GL_DRAW_FRAMEBUFFER) != GL_FRAMEBUFFER_COMPLETE) {
            ok = FALSE;
            break;
        }
        GLuint prog = i ? b->program_uv : b->program_y;
        glUseProgram(prog);
        glUniform1i(i ? b->uv_input : b->y_input, 0);
        glUniform1i(i ? b->uv_flip : b->y_flip, flip ? 1 : 0);
        glUniform1i(i ? b->uv_height : b->y_height, b->stream->config.height);
        glBindTexture(GL_TEXTURE_2D, source);
        glViewport(0, 0, i ? b->stream->config.width / 2 : b->stream->config.width,
                    i ? b->stream->config.height / 2 : b->stream->config.height);
        glDrawArrays(GL_TRIANGLES, 0, 3);
        ok = glGetError() == GL_NO_ERROR;
    }
    /* VA must not consume the surface until all GL writes have completed. */
    if (ok) glFinish();
    /* Detach GPU imported textures from the temporary framebuffer. */
    glFramebufferTexture2D(GL_DRAW_FRAMEBUFFER, GL_COLOR_ATTACHMENT0,
                           GL_TEXTURE_2D, 0, 0);
    glBindTexture(GL_TEXTURE_2D, 0);
    if (planes[0]) glDeleteTextures(1, &planes[0]);
    if (planes[1]) glDeleteTextures(1, &planes[1]);
    for (int i=0;i<2;i++) if (images[i] != EGL_NO_IMAGE_KHR)
        b->destroy_image(b->egl_display, images[i]);
    restore_gl(&saved);
    return ok;
}

/* Validate the *actual* VA DMA-BUF format/modifier and render target at
 * startup, not on the first live frame. Some VA drivers advertise import
 * support but allocate surfaces that are not EGL color-renderable. */
static gboolean preflight_import(VALinux *b) {
    AVFrame *frame = av_frame_alloc();
    if (!frame) return FALSE;
    int result = av_hwframe_get_buffer(b->hw_frames, frame, 0);
    if (result < 0) {
        av_frame_free(&frame);
        return FALSE;
    }
    VADRMPRIMESurfaceDescriptor desc = {0};
    VAStatus status = vaExportSurfaceHandle(
        b->va_display,
        (VASurfaceID)(uintptr_t)frame->data[3],
        VA_SURFACE_ATTRIB_MEM_TYPE_DRM_PRIME_2,
        VA_EXPORT_SURFACE_WRITE_ONLY | VA_EXPORT_SURFACE_SEPARATE_LAYERS,
        &desc);
    if (status != VA_STATUS_SUCCESS) {
        av_frame_free(&frame);
        return FALSE;
    }
    GLint old_active = 0, old_texture = 0;
    glGetIntegerv(GL_ACTIVE_TEXTURE, &old_active);
    glActiveTexture(GL_TEXTURE0);
    glGetIntegerv(GL_TEXTURE_BINDING_2D, &old_texture);
    GLuint texture = 0;
    glGenTextures(1, &texture);
    glBindTexture(GL_TEXTURE_2D, texture);
    glTexImage2D(GL_TEXTURE_2D, 0, GL_RGBA8,
        b->stream->config.width, b->stream->config.height,
        0, GL_RGBA, GL_UNSIGNED_BYTE, NULL); // GPU allocation; NO readback
    gboolean ok = texture && glGetError() == GL_NO_ERROR;
    if (ok) ok = upload_nv12_gpu(b, &desc, texture, FALSE);
    if (texture) glDeleteTextures(1, &texture);
    glBindTexture(GL_TEXTURE_2D, (GLuint)old_texture);
    glActiveTexture((GLenum)old_active);
    for (uint32_t i=0; i<desc.num_objects; ++i)
        if (desc.objects[i].fd >= 0) close(desc.objects[i].fd);
    av_frame_free(&frame);
    return ok;
}

static gboolean encoded_annexb(const uint8_t *data, int bytes) {
    return bytes >= 4 && data[0] == 0 && data[1] == 0 &&
           (data[2] == 1 || (data[2] == 0 && data[3] == 1));
}
static MCResult receive_packets(MCStream *s, VALinux *b) {
    for (;;) {
        int err = avcodec_receive_packet(b->encoder, b->packet);
        if (err == AVERROR(EAGAIN) || err == AVERROR_EOF) return MC_OK;
        if (err < 0) { log_ffmpeg(s,"VA-API receive packet",err); return MC_ERR_ENCODER; }
        AVPacket *p = b->packet;
        if (!encoded_annexb(p->data, p->size)) {
            mc_set_error(s,"VA-API produced non-Annex-B bitstream; not forwarding invalid data");
            av_packet_unref(p); return MC_ERR_ENCODER;
        }
        /* FFmpeg normally includes parameter sets in the bitstream. When it
         * returns Annex-B extradata, repeat it on keyframes for RTSP joiners. */
        const uint8_t *extra = b->encoder->extradata;
        int extra_len = b->encoder->extradata_size;
        gboolean key = (p->flags & AV_PKT_FLAG_KEY) != 0;
        gboolean prepend = key && extra && encoded_annexb(extra, extra_len);
        gsize total = (gsize)p->size + (prepend ? (gsize)extra_len : 0u);
        GstBuffer *out = gst_buffer_new_allocate(NULL, total, NULL);
        if (!out) { av_packet_unref(p); return MC_ERR_GSTREAMER; }
        gsize off = 0;
        if (prepend) { gst_buffer_fill(out, 0, extra, extra_len); off = extra_len; }
        gst_buffer_fill(out, off, p->data, p->size);
        gint64 pts = p->pts != AV_NOPTS_VALUE ? p->pts : p->dts;
        if (pts >= 0) {
            GST_BUFFER_PTS(out) = (GstClockTime)pts * 1000u;
            GST_BUFFER_DTS(out) = GST_BUFFER_PTS(out);
        }
        GST_BUFFER_DURATION(out) = gst_util_uint64_scale(1, GST_SECOND, s->config.fps);
        if (!key) GST_BUFFER_FLAG_SET(out, GST_BUFFER_FLAG_DELTA_UNIT);
        av_packet_unref(p);
        MCResult r = mc_output_push(s, out);
        if (r != MC_OK) return r;
    }
}

static MCResult start_compat(MCStream *s, const char *reason) {
    g_printerr("[Meatcorps.VideoStreaming][Linux][WARNING] "
               "VA-API / DMA-BUF GPU path unavailable: %s\n"
               "[Meatcorps.VideoStreaming][Linux][WARNING] "
               "Using prior GStreamer compatibility backend. "
               "CPU readback and/or software encoder may be used; see its encoder log.\n",
               reason && *reason ? reason : "unknown");
    MCResult result = mc_linux_compat_start(s);
    if (result == MC_OK) track_compat(s, TRUE);
    return result;
}

MCResult mc_backend_start(MCStream *s) {
    if (s->config.encoder == MC_ENCODER_NVENC ||
        s->config.encoder == MC_ENCODER_QSV ||
        s->config.encoder == MC_ENCODER_AMF ||
        s->config.encoder == MC_ENCODER_VIDEOTOOLBOX)
        return start_compat(s, "different hardware encoder selected");
    VALinux *b = g_new0(VALinux, 1);
    b->stream = s;
    s->backend = b;
    if (!va_init(s, b) || !preflight_import(b)) {
        if (!s->error[0])
            mc_set_error(s, "VA-API DMA-BUF preflight failed (format/modifier/FBO)");
        char reason[512];
        g_strlcpy(reason, s->error, sizeof(reason));
        destroy_va(b);
        s->backend = NULL;
        return start_compat(s, reason);
    }
    MCResult r = mc_output_start(s);
    if (r != MC_OK) {
        mc_output_destroy(s);
        destroy_va(b);
        s->backend = NULL;
        return r;
    }
    s->clock_start_us = g_get_monotonic_time();
    b->next_us = s->clock_start_us;
    g_print("[Meatcorps.VideoStreaming][Linux] "
            "OpenGL -> EGL DMA-BUF -> VA-API %s (GPU-only, no pixel readback)\n",
            s->config.codec == MC_CODEC_H265 ? "HEVC" : "H.264");
    return MC_OK;
}

MCResult mc_backend_submit(MCStream *s, uint32_t texture_id, int32_t flip_y) {
    if (is_compat(s)) return mc_linux_compat_submit(s,texture_id,flip_y);
    VALinux *b = s->backend;
    if (!b) return MC_ERR_STATE;
    void *current = b->raylib_egl ? (void *)eglGetCurrentContext()
                                  : (void *)glXGetCurrentContext();
    if (current != b->raylib_context) {
        mc_set_error(s, "VA-API Submit requires the original OpenGL context");
        return MC_ERR_GL;
    }
    gint64 now = g_get_monotonic_time();
    if (now < b->next_us) return MC_FRAME_SKIPPED;
    b->next_us = now + 1000000 / s->config.fps;
    AVFrame *frame = av_frame_alloc();
    if (!frame) return MC_ERR_ENCODER;
    int err = av_hwframe_get_buffer(b->hw_frames, frame, 0);
    if (err < 0) {
        log_ffmpeg(s, "VA-API frame allocation", err);
        av_frame_free(&frame); return MC_ERR_ENCODER;
    }
    VADRMPRIMESurfaceDescriptor desc = {0};
    VASurfaceID surface = (VASurfaceID)(uintptr_t)frame->data[3];
    VAStatus va = vaExportSurfaceHandle(b->va_display, surface,
        VA_SURFACE_ATTRIB_MEM_TYPE_DRM_PRIME_2,
        VA_EXPORT_SURFACE_WRITE_ONLY | VA_EXPORT_SURFACE_SEPARATE_LAYERS,
        &desc);
    if (va != VA_STATUS_SUCCESS) {
        mc_set_error(s, "VA-API export failed: %s", vaErrorStr(va));
        av_frame_free(&frame); return MC_ERR_ENCODER;
    }
    gboolean ok = upload_nv12_gpu(b, &desc, texture_id, flip_y != 0);
    for (uint32_t i = 0; i < desc.num_objects; ++i)
        if (desc.objects[i].fd >= 0) close(desc.objects[i].fd);
    if (!ok) {
        mc_set_error(s, "VA-API DMA-BUF plane import or GPU color conversion failed "
                        "(check EGL modifiers and render-node GPU selection)");
        av_frame_free(&frame); return MC_ERR_GL;
    }
    frame->pts = now - s->clock_start_us; // encoder time_base = microseconds
    err = avcodec_send_frame(b->encoder, frame);
    av_frame_free(&frame); // encoder retains necessary refs
    if (err == AVERROR(EAGAIN)) {
        MCResult r = receive_packets(s,b);
        if (r != MC_OK) return r;
        /* VA encoder may be full; dropping this one frame is better than blocking. */
        return MC_FRAME_DROPPED;
    }
    if (err < 0) { log_ffmpeg(s,"VA-API submit",err); return MC_ERR_ENCODER; }
    return receive_packets(s,b);
}

MCResult mc_backend_stop(MCStream *s) {
    if (is_compat(s)) {
        MCResult result = mc_linux_compat_stop(s);
        track_compat(s,FALSE);
        return result;
    }
    VALinux *b = s->backend;
    if (!b) return MC_OK;
    MCResult result = MC_OK;
    if (b->encoder) {
        int err = avcodec_send_frame(b->encoder, NULL);
        if (err >= 0) result = receive_packets(s,b);
        else { log_ffmpeg(s,"VA-API encoder flush",err); result = MC_ERR_ENCODER; }
    }
    if (s->output) {
        if (mc_output_stop(s) != MC_OK) result = MC_ERR_GSTREAMER;
        mc_output_destroy(s);
    }
    destroy_va(b);
    s->backend = NULL;
    return result;
}

void mc_backend_destroy(MCStream *s) {
    if (is_compat(s)) mc_linux_compat_destroy(s);
}
