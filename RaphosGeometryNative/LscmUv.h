#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult LscmUv(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
        [Out] out IntPtr uv, [Out] out long nuv);
    // uv is interleaved (u,v) pairs, one per vertex; nuv = number of vertices.
*/
RAPHOS_EXPORT
int LscmUv(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double** uv, Long& nuv
);
