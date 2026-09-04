#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult AutoUvAtlas(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
        [In] double hardAngle,
        [Out] out IntPtr uv, [Out] out long nCornerUv);
    // uv is interleaved (u,v) per face-corner (3 per triangle, in facet order); nCornerUv = corner count.
*/
RAPHOS_EXPORT
int AutoUvAtlas(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double hardAngle,
    double** uv, Long& nCornerUv
);
