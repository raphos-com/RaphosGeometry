#pragma once
#include "utils.h"

RAPHOS_EXPORT
int AlphaShape(
    double* pnts,
    Long pntCount,
    Long** facesPntIndices,
    Long& facesCount,
    double alpha
);
