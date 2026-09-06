#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult FillHoles(
        [In] IntPtr pnts, [In] long nv,
        [In] IntPtr faces, [In] long nf,
        [In] double maxHoleArea, [In] long maxHoleEdges,
        [Out] out IntPtr oV, [Out] out long onv,
        [Out] out IntPtr oF, [Out] out long onf,
        [Out] out long onPatch,
        [Out] out IntPtr oPatchVCount, [Out] out IntPtr oPatchFCount,
        [Out] out IntPtr oPV, [Out] out long oPVtotal,
        [Out] out IntPtr oPF, [Out] out long oPFtotal
    );
*/
RAPHOS_EXPORT
int FillHoles(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double maxHoleArea, Long maxHoleEdges,
    double** oV, Long& onv,
    Long** oF, Long& onf,
    Long& onPatch,
    Long** oPatchVCount, Long** oPatchFCount,
    double** oPV, Long& oPVtotal,
    Long** oPF, Long& oPFtotal
);
