#pragma once
#include "utils.h"
#include <cstddef>

/*
    internal static extern RaphosInteropResult FlipoutGeodesic(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
        [In] long startIdx, [In] long endIdx,
        [Out] out IntPtr newPnts, [Out] out long newPntCount);
*/
RAPHOS_EXPORT
int FlipoutGeodesic(
    double* pnts, Long nv,
    size_t* faces, Long nf,
    size_t startIdx, size_t endIdx,
    double** newPnts, Long& newPntCount
);
