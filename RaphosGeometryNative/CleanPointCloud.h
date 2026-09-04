#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult CleanPointCloud(
        [In] IntPtr pnts, [In] long pntCount,
        [Out] IntPtr newPnts,          // caller-allocated, same capacity as input
        [Out] IntPtr newPntsCount,     // out: number of surviving points
        [In] long n, [In] double r);
*/
RAPHOS_EXPORT
int CleanPointCloud(
    double* pnts, Long pntCount,
    double* newPnts, Long* newPntsCount,
    Long n, double r
);
