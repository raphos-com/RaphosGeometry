#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult HausdorffDistance(
        [In] IntPtr va, [In] long nva, [In] IntPtr fa, [In] long nfa,
        [In] IntPtr vb, [In] long nvb, [In] IntPtr fb, [In] long nfb,
        [Out] out double dAB, [Out] out double dBA, [Out] out double dSym);
*/
RAPHOS_EXPORT
int HausdorffDistance(
    double* va, Long nva, Long* fa, Long nfa,
    double* vb, Long nvb, Long* fb, Long nfb,
    double& dAB, double& dBA, double& dSym
);
