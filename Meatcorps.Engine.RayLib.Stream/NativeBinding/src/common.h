#ifndef MC_COMMON_H
#define MC_COMMON_H
#include "backend.h"

// Both output modes receive ALREADY-ENCODED Annex-B access units.
MCResult mc_output_start(MCStream *stream);
MCResult mc_output_push_audio(MCStream *stream, const float *samples, uint32_t frames, uint64_t pts_ns);
MCResult mc_output_push(MCStream *stream, GstBuffer *buffer); // transfers ownership
MCResult mc_output_stop(MCStream *stream);
void mc_output_destroy(MCStream *stream);

#endif
