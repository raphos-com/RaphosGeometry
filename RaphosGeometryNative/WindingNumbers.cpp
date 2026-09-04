#include "WindingNumbers.h"
#include "EigenMeshUtils.h"
#include <igl/winding_number.h>

// Generalized winding number: for each query point, ~1 inside a closed mesh and
// ~0 outside, robust on imperfect/open meshes where a BREP inside/outside test fails.
int WindingNumbers(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double* query, Long nq,
    double** w, Long& onw
) {
    using namespace RaphosGeo;

    Eigen::MatrixXd V;
    Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::MatrixXd O(nq, 3);
    for (Long i = 0; i < nq; i++)
    {
        O(i, 0) = query[i * 3 + 0];
        O(i, 1) = query[i * 3 + 1];
        O(i, 2) = query[i * 3 + 2];
    }

    Eigen::VectorXd W;
    igl::winding_number(V, F, O, W);

    WriteDoubles(W.data(), (Long)W.size(), w, onw);
    return RAPHOS_SUCCESS;
}
