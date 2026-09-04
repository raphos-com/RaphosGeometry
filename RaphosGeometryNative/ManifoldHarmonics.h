#pragma once
#include "utils.h"

/*
    internal static extern RaphosInteropResult ManifoldHarmonics(
        [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf, [In] long nbEigens,
        [Out] out IntPtr eigenvalues, [Out] out long nEigenvalues,
        [Out] out IntPtr eigenvectors, [Out] out long nEigenvectors);
    // eigenvectors is nbEigens*nv doubles: value of eigenfunction j at vertex i = eigenvectors[j*nv + i].
*/
RAPHOS_EXPORT
int ManifoldHarmonics(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long nbEigens,
    double** eigenvalues, Long& nEigenvalues,
    double** eigenvectors, Long& nEigenvectors
);
