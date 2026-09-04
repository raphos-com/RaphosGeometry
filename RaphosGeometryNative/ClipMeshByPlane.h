#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult ClipMeshByPlane(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
        [In] double px, [In] double py, [In] double pz,
        [In] double nx, [In] double ny, [In] double nz,
        [Out] out IntPtr oV, [Out] out long onv, [Out] out IntPtr oF, [Out] out long onf);
    // Keeps the half of the mesh on the negative side of the plane (dot(v - p, n) <= 0).
*/
RAPHOS_EXPORT
int ClipMeshByPlane(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double px, double py, double pz,
    double nx, double ny, double nz,
    double** oV, Long& onv,
    Long** oF, Long& onf
);
