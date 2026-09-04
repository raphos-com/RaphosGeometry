#pragma once
#include "utils.h"
#include <cstddef>

/*
    internal static extern RaphosInteropResult HeatGeodesicField(
        [In] IntPtr pnts, [In] long nv,
        [In] IntPtr faces, [In] long nf,
        [In] IntPtr sources, [In] long nsources,
        [Out] out IntPtr distances, [Out] out long ndist
    );
*/
RAPHOS_EXPORT
int HeatGeodesicField(
    double* pnts, Long nv,
    size_t* faces, Long nf,
    size_t* sources, Long nsources,
    double** distances, Long& ndist
);
