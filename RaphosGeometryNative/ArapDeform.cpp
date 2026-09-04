#include "ArapDeform.h"
#include "EigenMeshUtils.h"
#include <igl/arap.h>

// As-rigid-as-possible handle-based deformation (libigl): the handle vertices are constrained to
// the given target positions and the rest of the mesh follows as rigidly as possible.
int ArapDeform(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long* handles, Long nh,
    double* targets,
    Long iterations,
    double** oV, Long& onv
) {
    using namespace RaphosGeo;

    Eigen::MatrixXd V; Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::VectorXi b(nh);
    for (Long i = 0; i < nh; i++) b(i) = (int)handles[i];

    Eigen::MatrixXd bc(nh, 3);
    for (Long i = 0; i < nh; i++)
    {
        bc(i, 0) = targets[i * 3 + 0];
        bc(i, 1) = targets[i * 3 + 1];
        bc(i, 2) = targets[i * 3 + 2];
    }

    igl::ARAPData data;
    data.max_iter = (int)(iterations > 0 ? iterations : 100);
    if (!igl::arap_precomputation(V, F, 3, b, data)) return RAPHOS_ERROR;

    Eigen::MatrixXd U = V;                       // initial guess = rest pose
    if (!igl::arap_solve(bc, data, U)) return RAPHOS_ERROR;

    WriteEigenVerts(U, oV, onv);
    return RAPHOS_SUCCESS;
}
