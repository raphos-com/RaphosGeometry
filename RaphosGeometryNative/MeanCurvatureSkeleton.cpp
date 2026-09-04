#include "MeanCurvatureSkeleton.h"
#include "EigenMeshUtils.h"
#include <igl/cotmatrix.h>
#include <igl/massmatrix.h>
#include <Eigen/Sparse>
#include <Eigen/SparseCholesky>

// Contract the mesh with implicit mean-curvature flow: (M - dt*L) V' = M V, iterated. L is the
// (negative-semidefinite) cotangent Laplacian, so the system is SPD. Tubular parts collapse toward
// their centrelines, yielding a curve-skeleton-like point set on the same connectivity.
int MeanCurvatureSkeleton(
    double* pnts, Long nv, Long* faces, Long nf,
    Long iterations, double stepScale,
    double** oV, Long& onv
) {
    using namespace RaphosGeo;
    Eigen::MatrixXd V; Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    int iters = (int)(iterations > 0 ? iterations : 5);
    double dt = stepScale > 0 ? stepScale : 0.1;

    for (int it = 0; it < iters; it++) {
        Eigen::SparseMatrix<double> L, M;
        igl::cotmatrix(V, F, L);
        igl::massmatrix(V, F, igl::MASSMATRIX_TYPE_BARYCENTRIC, M);
        // Scale dt by mean cell area so the step is geometry-relative and stable as it contracts.
        double meanArea = M.diagonal().sum() / (double)V.rows();
        Eigen::SparseMatrix<double> A = M - (dt * meanArea) * L;
        Eigen::SimplicialLDLT<Eigen::SparseMatrix<double>> solver(A);
        if (solver.info() != Eigen::Success) break;
        Eigen::MatrixXd rhs = M * V;
        Eigen::MatrixXd Vn = solver.solve(rhs);
        if (solver.info() != Eigen::Success) break;
        V = Vn;
    }

    WriteEigenVerts(V, oV, onv);
    return RAPHOS_SUCCESS;
}
