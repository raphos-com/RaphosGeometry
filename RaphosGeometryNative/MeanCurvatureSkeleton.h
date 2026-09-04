#pragma once
#include "utils.h"

// Mesh contraction toward the mean-curvature skeleton (Au et al. 2008) via implicit Laplacian
// (mean-curvature) flow. Faces are unchanged; vertices are moved onto the contracted skeleton.
RAPHOS_EXPORT
int MeanCurvatureSkeleton(
    double* pnts, Long nv, Long* faces, Long nf,
    Long iterations, double stepScale,
    double** oV, Long& onv
);
