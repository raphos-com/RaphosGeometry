#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult FillHoles(
        [In] IntPtr pnts, [In] long nv,
        [In] IntPtr faces, [In] long nf,
        [In] double maxHoleArea, [In] long maxHoleEdges,
        [Out] out IntPtr oV, [Out] out long onv,
        [Out] out IntPtr oF, [Out] out long onf
    );
*/
RAPHOS_EXPORT
int FillHoles(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double maxHoleArea, Long maxHoleEdges,
    double** oV, Long& onv,
    Long** oF, Long& onf
);
