#include "ManifoldHarmonics.h"
#include "EigenMeshUtils.h"
#include <igl/cotmatrix.h>
#include <igl/massmatrix.h>
#include <Eigen/Dense>
#include <Eigen/Eigenvalues>
#include <vector>
#include <algorithm>

// Laplace-Beltrami eigenfunctions ("manifold harmonics"). Geogram's spectral solver needs the
// OpenNL ARPACK extension, which is not in the prebuilt geogram.lib, so this is a clean permissive
// reimplementation: build the cotangent Laplacian L and Voronoi mass matrix M (libigl), then solve
// the generalized symmetric eigenproblem (-L) x = lambda M x with Eigen's dense solver. Suitable for
// small/medium meshes (dense O(n^3)); large meshes would want a sparse iterative solver.
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

    Eigen::MatrixXd A = Eigen::MatrixXd(-L);                  // PSD
    Eigen::MatrixXd B = Eigen::MatrixXd(M);

    // Generalized symmetric eigenproblem A x = lambda B x, eigenvalues ascending.
    Eigen::GeneralizedSelfAdjointEigenSolver<Eigen::MatrixXd> es(A, B);
    if (es.info() != Eigen::Success)
        return RAPHOS_ERROR;

    int total = (int)es.eigenvalues().size();
    int k = (int)(nbEigens > 0 ? nbEigens : 10);
    if (k > total) k = total;

    nEigenvalues = k;
    double* valBuf = new double[k > 0 ? k : 1];
    for (int i = 0; i < k; i++) valBuf[i] = es.eigenvalues()(i);
    *eigenvalues = valBuf;

    // eigenvectors: column i is the i-th eigenfunction over the vertices.
    nEigenvectors = (Long)k * nv;
    double* vecBuf = new double[nEigenvectors > 0 ? nEigenvectors : 1];
    for (int b = 0; b < k; b++)
        for (Long i = 0; i < nv; i++)
            vecBuf[(Long)b * nv + i] = es.eigenvectors()(i, b);
    *eigenvectors = vecBuf;

    return RAPHOS_SUCCESS;
}
