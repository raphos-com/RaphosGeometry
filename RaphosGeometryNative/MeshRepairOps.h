#pragma once
#include "utils.h"

// Geogram-backed mesh repair family. All take/return interleaved triangle meshes.

RAPHOS_EXPORT
int RepairMesh(
    double* pnts, Long nv, Long* faces, Long nf,
    double colocateEpsilon, bool triangulate,
    double** oV, Long& onv, Long** oF, Long& onf);

RAPHOS_EXPORT
int MakeConsistent(
    double* pnts, Long nv, Long* faces, Long nf,
    double** oV, Long& onv, Long** oF, Long& onf);

RAPHOS_EXPORT
int RemoveSelfIntersections(
    double* pnts, Long nv, Long* faces, Long nf,
    Long maxIter,
    double** oV, Long& onv, Long** oF, Long& onf);
