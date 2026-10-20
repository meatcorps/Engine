#ifndef MC_BACKEND_H
#define MC_BACKEND_H
#include "mc_stream.h"
#include <gst/gst.h>
#include <gst/app/gstappsrc.h>

// Backend-specific state stays entirely in the selected platform/backend.c.
typedef struct MCOutputState MCOutputState;

struct MCStream {
    MCConfig config;
    char *location_owned;
    char error[512];
    int started;
    void *backend;
    MCOutputState *output;
    gint64 clock_start_us;
};

void mc_set_error(MCStream *stream, const char *format, ...);
MCResult mc_backend_start(MCStream *stream);
MCResult mc_backend_submit(MCStream *stream, uint32_t texture_id, int32_t flip_y);
MCResult mc_backend_stop(MCStream *stream);
void mc_backend_destroy(MCStream *stream);
#endif
