#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult RansacDetect(
        [In] IntPtr pnts, [In] long nv,
        [In] double distThreshold, [In] long minSupport, [In] long iterations,
        [Out] out IntPtr labels, [Out] out long nLabels,
        [Out] out IntPtr types, [Out] out long nTypes);
    // labels[i] = primitive index for point i (-1 = unassigned).
    // types[k]  = primitive type code (0 plane, 1 sphere, 2 cylinder).
*/
RAPHOS_EXPORT
int RansacDetect(
    double* pnts, Long nv,
    double distThreshold, Long minSupport, Long iterations,
    Long** labels, Long& nLabels,
    Long** types, Long& nTypes
);
