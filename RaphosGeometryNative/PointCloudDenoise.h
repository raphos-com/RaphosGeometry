#pragma once
#include "utils.h"

// Weighted Locally Optimal Projection (Huang et al. 2009): consolidate/denoise a point set by
// iterated attraction to the input plus inter-point repulsion. Output has the same count.
RAPHOS_EXPORT
int WlopConsolidate(
    double* pnts, Long nv, Long iterations, double radius,
    double** out, Long& nout
);

// Bilateral point-cloud denoising: move each point along its normal by a bilateral-weighted
// average of neighbour offsets (spatial + normal Gaussians). Requires per-point normals.
RAPHOS_EXPORT
int BilateralDenoise(
    double* pnts, double* normals, Long nv,
    double sigmaSpace, double sigmaNormal, Long iterations,
    double** out, Long& nout
);
