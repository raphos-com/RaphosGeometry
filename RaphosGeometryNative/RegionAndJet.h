#pragma once
#include "utils.h"

// Region-growing segmentation of a point cloud into smooth (near-planar) regions by normal
// similarity over the k-NN graph. labels[i] = region index (-1 if in a region below minRegion).
RAPHOS_EXPORT
int RegionGrowing(
    double* pnts, Long nv,
    double angleDeg, Long k, Long minRegion,
    Long** labels, Long& nLabels, Long& nRegions
);

// Jet (Monge quadric) fitting per point -> principal curvatures k1 (max) and k2 (min).
// Cazals-Pouget style local polynomial fit in a PCA frame. Ridge strength = |k1|.
RAPHOS_EXPORT
int JetCurvature(
    double* pnts, Long nv, Long k,
    double** k1, double** k2, Long& nout
);
