#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult PoissonReconstruct(
        [In] IntPtr pnts, [In] IntPtr normals, [In] long nv, [In] long depth,
        [Out] out IntPtr oV, [Out] out long onv, [Out] out IntPtr oF, [Out] out long onf);
    // Normals are required (Poisson reconstruction is normal-driven).
*/
RAPHOS_EXPORT
int PoissonReconstruct(
    double* pnts, double* normals, Long nv,
    Long depth,
    double** oV, Long& onv,
    Long** oF, Long& onf
);
