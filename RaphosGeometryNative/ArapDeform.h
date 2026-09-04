#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult ArapDeform(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
        [In] IntPtr handles, [In] long nh, [In] IntPtr targets, [In] long iterations,
        [Out] out IntPtr oV, [Out] out long onv);
    // handles: nh vertex indices; targets: nh interleaved xyz target positions.
    // Faces are unchanged; only vertex positions are returned.
*/
RAPHOS_EXPORT
int ArapDeform(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long* handles, Long nh,
    double* targets,
    Long iterations,
    double** oV, Long& onv
);
