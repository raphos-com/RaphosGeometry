#pragma once
#include "utils.h"
#include <cstddef>

/*
    internal static extern RaphosInteropResult VectorHeatTransport(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
        [In] long sourceIdx, [In] double svx, [In] double svy, [In] double svz,
        [Out] out IntPtr vectors, [Out] out long nvectors);
    // Source direction is world-space; it is projected onto the source vertex's tangent basis.
    // Output: one world-space 3D vector per vertex (interleaved xyz).
*/
RAPHOS_EXPORT
int VectorHeatTransport(
    double* pnts, Long nv,
    size_t* faces, Long nf,
    size_t sourceIdx, double svx, double svy, double svz,
    double** vectors, Long& nvectors
);
