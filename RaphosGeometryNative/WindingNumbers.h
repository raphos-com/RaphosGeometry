#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult WindingNumbers(
        [In] IntPtr pnts, [In] long nv,
        [In] IntPtr faces, [In] long nf,
        [In] IntPtr query, [In] long nq,
        [Out] out IntPtr w, [Out] out long onw
    );
*/
RAPHOS_EXPORT
int WindingNumbers(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double* query, Long nq,
    double** w, Long& onw
);
