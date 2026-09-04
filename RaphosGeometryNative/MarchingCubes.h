#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult MarchingCubes(
        [In] IntPtr scalars, [In] long ns,
        [In] IntPtr gridVerts,
        [In] long nx, [In] long ny, [In] long nz,
        [In] double isovalue,
        [Out] out IntPtr oV, [Out] out long onv,
        [Out] out IntPtr oF, [Out] out long onf
    );
*/
RAPHOS_EXPORT
int MarchingCubes(
    double* scalars, Long ns,
    double* gridVerts,
    Long nx, Long ny, Long nz,
    double isovalue,
    double** oV, Long& onv,
    Long** oF, Long& onf
);
