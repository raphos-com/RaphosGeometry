#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult PrincipalCurvature(
        [In] IntPtr pnts, [In] long nv,
        [In] IntPtr faces, [In] long nf,
        [In] long radius,
        [Out] out IntPtr pd1, [Out] out IntPtr pd2,
        [Out] out IntPtr pv1, [Out] out IntPtr pv2,
        [Out] out long onv
    );
*/
RAPHOS_EXPORT
int PrincipalCurvature(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long radius,
    double** pd1, double** pd2,
    double** pv1, double** pv2,
    Long& onv
);
