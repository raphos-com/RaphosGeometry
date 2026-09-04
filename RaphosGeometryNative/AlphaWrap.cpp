#include "AlphaWrap.h"
#include "EigenMeshUtils.h"
#include <igl/signed_distance.h>
#include <igl/marching_cubes.h>
#include <algorithm>

// Sample a signed-distance field (fast winding number sign) on a padded regular grid around the
// input, then extract the isosurface at +offset with marching cubes -> a watertight envelope.
int AlphaWrap(
    double* pnts, Long nv, Long* faces, Long nf,
    double offset, Long resolution,
    double** oV, Long& onv, Long** oF, Long& onf
) {
    using namespace RaphosGeo;
    Eigen::MatrixXd V; Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::RowVector3d lo = V.colwise().minCoeff();
    Eigen::RowVector3d hi = V.colwise().maxCoeff();
    double off = offset > 0 ? offset : (hi - lo).norm() * 0.02;
    double pad = off * 2.0 + (hi - lo).norm() * 0.03;
    lo.array() -= pad; hi.array() += pad;

    int res = (int)(resolution >= 8 ? resolution : 48);
    // Build grid points in x-fastest order (matches marching_cubes' expected layout).
    long n = (long)res * res * res;
    Eigen::MatrixXd GV(n, 3);
    long idx = 0;
    for (int k = 0; k < res; k++)
        for (int j = 0; j < res; j++)
            for (int i = 0; i < res; i++) {
                GV(idx, 0) = lo(0) + (hi(0) - lo(0)) * i / (res - 1);
                GV(idx, 1) = lo(1) + (hi(1) - lo(1)) * j / (res - 1);
                GV(idx, 2) = lo(2) + (hi(2) - lo(2)) * k / (res - 1);
                idx++;
            }

    Eigen::VectorXd S; Eigen::VectorXi I; Eigen::MatrixXd C, N;
    igl::signed_distance(GV, V, F, igl::SIGNED_DISTANCE_TYPE_FAST_WINDING_NUMBER,
        std::numeric_limits<double>::lowest(), std::numeric_limits<double>::max(), S, I, C, N);

    Eigen::MatrixXd oVe; Eigen::MatrixXi oFe;
    igl::marching_cubes(S, GV, (unsigned)res, (unsigned)res, (unsigned)res, off, oVe, oFe);

    WriteEigenVerts(oVe, oV, onv);
    WriteEigenFaces(oFe, oF, onf);
    return RAPHOS_SUCCESS;
}
