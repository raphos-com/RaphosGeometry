#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult AdvancingFront(
        [In] IntPtr pnts, [In] long nv, [In] double radius,
        [Out] out IntPtr faces, [Out] out long faceCount);
    // Faces index the input points. radius = pivoting ball radius (0 -> auto from spacing).
*/
RAPHOS_EXPORT
int AdvancingFront(
    double* pnts, Long nv, double radius,
    Long** faces, Long& faceCount
);
