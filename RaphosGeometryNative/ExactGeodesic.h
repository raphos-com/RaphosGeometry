#pragma once
#include "utils.h"
#include <cstddef>

/*
    internal static extern RaphosInteropResult ExactGeodesic(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
        [In] IntPtr sources, [In] long nsources,
        [Out] out IntPtr distances, [Out] out long ndist);
*/
RAPHOS_EXPORT
int ExactGeodesic(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long* sources, Long nsources,
    double** distances, Long& ndist
);
