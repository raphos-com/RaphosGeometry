#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult EstimateNormals(
        [In] IntPtr pnts, [In] long nv, [In] long k,
        [Out] out IntPtr normals, [Out] out long nnormals);
*/
RAPHOS_EXPORT
int EstimateNormals(
    double* pnts, Long nv,
    Long k,
    double** normals, Long& nnormals
);
