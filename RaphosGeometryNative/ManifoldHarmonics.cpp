#include "ManifoldHarmonics.h"
#include "EigenMeshUtils.h"
#include <igl/cotmatrix.h>
#include <igl/massmatrix.h>
#include <geometrycentral/numerical/linear_solvers.h>
#include <Eigen/Sparse>
#include <vector>

// Laplace-Beltrami eigenfunctions ("manifold harmonics"). Build the cotangent Laplacian L and Voronoi
// mass matrix M (libigl), then find the k smallest eigenpairs of the generalized symmetric problem
// (-L) x = lambda M x. This uses geometry-central's sparse inverse-power-iteration solver
// (smallestKEigenvectorsPositiveDefinite) instead of Eigen's dense solver, so cost is ~O(k * nnz)
// with a single sparse factorization rather than dense O(n^3) — fast on full-resolution meshes.
int ManifoldHarmonics(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long nbEigens,
    double** eigenvalues, Long& nEigenvalues,
    double** eigenvectors, Long& nEigenvectors
) {
    using namespace RaphosGeo;

    Eigen::MatrixXd V; Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::SparseMatrix<double> L, M;
    igl::cotmatrix(V, F, L);                                  // symmetric negative-semidefinite
    igl::massmatrix(V, F, igl::MASSMATRIX_TYPE_VORONOI, M);   // diagonal, positive

    Eigen::SparseMatrix<double> A = -L;                       // PSD (null space = the constant vector)

    int k = (int)(nbEigens > 0 ? nbEigens : 10);
    if (k > (int)nv) k = (int)nv;
    if (k <= 0) return RAPHOS_ERROR;

    // A is only positive *semi*-definite (lambda_0 = 0 on constants), which the Cholesky factorization
    // inside the solver cannot handle, so shift by a tiny multiple of M: (A + shift*M) is positive
    // definite and shares A's eigenvectors; the shift moves each eigenvalue by +shift, which we undo
    // below by evaluating the Rayleigh quotient against the original A.
    const double shift = 1e-8;
    Eigen::SparseMatrix<double> Ashift = A + shift * M;

    std::vector<Eigen::VectorXd> vecs;
    try {
        vecs = geometrycentral::smallestKEigenvectorsPositiveDefinite<double>(Ashift, M, (size_t)k);
    } catch (...) {
        return RAPHOS_ERROR;
    }
    if ((int)vecs.size() < k) k = (int)vecs.size();
    if (k <= 0) return RAPHOS_ERROR;

    // eigenvalues: Rayleigh quotient against the ORIGINAL A (the vectors are M-orthonormalized, so the
    // denominator is ~1); ascending, with lambda_0 ~ 0 for the (near-)constant first eigenfunction.
    nEigenvalues = k;
    double* valBuf = new double[k];
    for (int i = 0; i < k; i++) {
        const Eigen::VectorXd& v = vecs[i];
        double den = v.dot(M * v);
        double num = v.dot(A * v);
        valBuf[i] = (den != 0.0) ? (num / den) : 0.0;
    }
    *eigenvalues = valBuf;

    // eigenvectors: column b is the b-th eigenfunction over the vertices, flattened as [b*nv + i].
    nEigenvectors = (Long)k * nv;
    double* vecBuf = new double[nEigenvectors > 0 ? nEigenvectors : 1];
    for (int b = 0; b < k; b++)
        for (Long i = 0; i < nv; i++)
            vecBuf[(Long)b * nv + i] = vecs[b](i);
    *eigenvectors = vecBuf;

    return RAPHOS_SUCCESS;
}
