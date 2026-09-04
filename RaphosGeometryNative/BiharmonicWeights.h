#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult BiharmonicWeights(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
        [In] IntPtr handles, [In] long nh,
        [Out] out IntPtr weights, [Out] out long nWeights, [Out] out long nHandlesOut);
    // weights is row-major nv*nh: weight of handle j at vertex i = weights[i*nh + j].
*/
RAPHOS_EXPORT
int BiharmonicWeights(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double* handles, Long nh,
    double** weights, Long& nWeights, Long& nHandlesOut
);
