#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult QuadricDecimate(
        [In] IntPtr pnts, [In] long nv,
        [In] IntPtr faces, [In] long nf,
        [In] long targetFaces,
        [Out] out IntPtr oV, [Out] out long onv,
        [Out] out IntPtr oF, [Out] out long onf
    );
*/
RAPHOS_EXPORT
int QuadricDecimate(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long targetFaces,
    double** oV, Long& onv,
    Long** oF, Long& onf
);
