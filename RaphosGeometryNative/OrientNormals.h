#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult OrientNormals(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr normals, [In] long k,
        [Out] out IntPtr outNormals, [Out] out long nout);
*/
RAPHOS_EXPORT
int OrientNormals(
    double* pnts, Long nv,
    double* normals, Long k,
    double** outNormals, Long& nout
);
