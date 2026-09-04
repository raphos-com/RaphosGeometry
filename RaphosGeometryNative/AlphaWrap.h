#pragma once
#include "utils.h"

// Watertight shrink-wrap of a (possibly messy) mesh: sample a signed-distance field on a grid and
// extract the offset isosurface with marching cubes. A permissive stand-in for CGAL alpha-wrap.
RAPHOS_EXPORT
int AlphaWrap(
    double* pnts, Long nv, Long* faces, Long nf,
    double offset, Long resolution,
    double** oV, Long& onv, Long** oF, Long& onf
);
