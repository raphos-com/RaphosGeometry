#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult RansacDetect(
        [In] IntPtr pnts, [In] long nv,
        [In] double distThreshold, [In] long minSupport, [In] long iterations,
        [Out] out IntPtr labels, [Out] out long nLabels,
        [Out] out IntPtr types, [Out] out long nTypes,
        [Out] out IntPtr aParams, [Out] out IntPtr bParams, [Out] out IntPtr radii);
    // labels[i] = primitive index for point i (-1 = unassigned).
    // types[k]  = primitive type code (0 plane, 1 sphere, 2 cylinder).
    // Per detected primitive k (nTypes of them), the fitted parameters:
    //   aParams[3k..3k+2] = plane: a point on it | sphere: centre | cylinder: a point on the axis.
    //   bParams[3k..3k+2] = plane: unit normal    | sphere: (0,0,0) | cylinder: unit axis direction.
    //   radii[k]          = plane: 0              | sphere: radius  | cylinder: radius.
*/
RAPHOS_EXPORT
int RansacDetect(
    double* pnts, Long nv,
    double distThreshold, Long minSupport, Long iterations,
    Long** labels, Long& nLabels,
    Long** types, Long& nTypes,
    double** aParams, double** bParams, double** radii
);
