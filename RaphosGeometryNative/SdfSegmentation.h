#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult SdfSegmentation(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf, [In] long nSegments,
        [Out] out IntPtr sdf, [Out] out long nSdf, [Out] out IntPtr labels, [Out] out long nLabels);
    // sdf[f] and labels[f] are per-face (one value per triangle).
*/
RAPHOS_EXPORT
int SdfSegmentation(
    double* pnts, Long nv, Long* faces, Long nf,
    Long nSegments,
    double** sdf, Long& nSdf,
    Long** labels, Long& nLabels
);
