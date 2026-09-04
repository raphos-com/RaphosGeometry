#pragma once
#include "utils.h"

// uv: interleaved (u,v) per vertex; nuv = vertex count.
RAPHOS_EXPORT
int HarmonicParam(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double** uv, Long& nuv
);

// As-rigid-as-possible UV parameterization (free boundary, harmonic initial guess).
RAPHOS_EXPORT
int ArapUv(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long iterations,
    double** uv, Long& nuv
);
