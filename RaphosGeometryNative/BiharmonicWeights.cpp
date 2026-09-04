#include "BiharmonicWeights.h"
#include "EigenMeshUtils.h"
#include <igl/bbw.h>
#include <igl/boundary_conditions.h>
#include <igl/normalize_row_sums.h>

// Bounded biharmonic weights (Jacobson et al. 2011): smooth, non-negative, partition-of-unity
// skinning weights for a set of point handles, solved with libigl's native active-set QP.
int BiharmonicWeights(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double* handles, Long nh,
    double** weights, Long& nWeights, Long& nHandlesOut
) {
    using namespace RaphosGeo;

    Eigen::MatrixXd V; Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::MatrixXd C(nh, 3);            // handle (control) positions
    for (Long i = 0; i < nh; i++)
    {
        C(i, 0) = handles[i * 3 + 0];
        C(i, 1) = handles[i * 3 + 1];
        C(i, 2) = handles[i * 3 + 2];
    }
    Eigen::VectorXi P(nh);               // all handles are point handles
    for (Long i = 0; i < nh; i++) P(i) = (int)i;
    Eigen::MatrixXi BE(0, 2), CE(0, 2);  // no bone or cage edges

    Eigen::VectorXi b;
    Eigen::MatrixXd bc;
    if (!igl::boundary_conditions(V, F, C, P, BE, CE, b, bc))
        return RAPHOS_ERROR;

    igl::BBWData data;
    data.active_set_params.max_iter = 8;
    data.verbosity = 0;

    Eigen::MatrixXd W;
    if (!igl::bbw(V, F, b, bc, data, W))
        return RAPHOS_ERROR;

    igl::normalize_row_sums(W, W);       // enforce partition of unity

    nHandlesOut = (Long)W.cols();
    nWeights = (Long)W.rows() * W.cols();
    double* buf = new double[nWeights > 0 ? nWeights : 1];
    for (Long i = 0; i < (Long)W.rows(); i++)
        for (Long j = 0; j < (Long)W.cols(); j++)
            buf[i * W.cols() + j] = W(i, j);
    *weights = buf;
    return RAPHOS_SUCCESS;
}
