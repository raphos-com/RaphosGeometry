#include "LscmUv.h"
#include "EigenMeshUtils.h"
#include <igl/lscm.h>
#include <igl/boundary_loop.h>

// Least-Squares Conformal Map UV unwrapping (libigl). Pins two boundary vertices to remove the
// free similarity transform: the first boundary vertex to (0,0) and the diametrically opposite
// one to (1,0). Requires the mesh to have a boundary (an open surface / disk topology).
int LscmUv(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double** uv, Long& nuv
) {
    using namespace RaphosGeo;

    Eigen::MatrixXd V;
    Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::VectorXi bnd;
    igl::boundary_loop(F, bnd);
    if (bnd.size() < 2)
        return RAPHOS_ERROR;                  // closed mesh: no boundary to pin

    Eigen::VectorXi b(2);
    b(0) = bnd(0);
    b(1) = bnd((int)(bnd.size() / 2));
    Eigen::MatrixXd bc(2, 2);
    bc << 0, 0, 1, 0;

    Eigen::MatrixXd V_uv;
    if (!igl::lscm(V, F, b, bc, V_uv))
        return RAPHOS_ERROR;

    nuv = (Long)V_uv.rows();
    double* buf = new double[nuv * 2];
    for (Long i = 0; i < nuv; i++)
    {
        buf[i * 2 + 0] = V_uv(i, 0);
        buf[i * 2 + 1] = V_uv(i, 1);
    }
    *uv = buf;
    return RAPHOS_SUCCESS;
}
