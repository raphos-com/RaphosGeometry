#include "HarmonicParam.h"
#include "EigenMeshUtils.h"
#include <igl/harmonic.h>
#include <igl/arap.h>
#include <igl/boundary_loop.h>
#include <igl/map_vertices_to_circle.h>

static void WriteUv(const Eigen::MatrixXd& UV, double** uv, Long& nuv)
{
    nuv = (Long)UV.rows();
    double* buf = new double[nuv * 2];
    for (Long i = 0; i < nuv; i++)
    {
        buf[i * 2 + 0] = UV(i, 0);
        buf[i * 2 + 1] = UV(i, 1);
    }
    *uv = buf;
}

// Fixed-boundary harmonic parameterization: pin the boundary loop onto a circle, then solve the
// harmonic (Laplace) system for the interior UVs (libigl).
int HarmonicParam(
    double* pnts, Long nv, Long* faces, Long nf,
    double** uv, Long& nuv
) {
    using namespace RaphosGeo;
    Eigen::MatrixXd V; Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::VectorXi bnd;
    igl::boundary_loop(F, bnd);
    if (bnd.size() < 3) return RAPHOS_ERROR;

    Eigen::MatrixXd bnd_uv;
    igl::map_vertices_to_circle(V, bnd, bnd_uv);

    Eigen::MatrixXd UV;
    if (!igl::harmonic(V, F, bnd, bnd_uv, 1, UV)) return RAPHOS_ERROR;

    WriteUv(UV, uv, nuv);
    return RAPHOS_SUCCESS;
}

// As-rigid-as-possible UV parameterization: harmonic UVs as the initial guess, then ARAP local/global
// iterations in 2D for low-distortion flattening (libigl).
int ArapUv(
    double* pnts, Long nv, Long* faces, Long nf,
    Long iterations,
    double** uv, Long& nuv
) {
    using namespace RaphosGeo;
    Eigen::MatrixXd V; Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::VectorXi bnd;
    igl::boundary_loop(F, bnd);
    if (bnd.size() < 3) return RAPHOS_ERROR;

    Eigen::MatrixXd bnd_uv;
    igl::map_vertices_to_circle(V, bnd, bnd_uv);

    Eigen::MatrixXd UV;
    if (!igl::harmonic(V, F, bnd, bnd_uv, 1, UV)) return RAPHOS_ERROR;   // initial guess

    igl::ARAPData arap_data;
    // Free-boundary ARAP parameterization is singular without regularization; dynamics anchors
    // each iteration to the previous solution, removing the global null space.
    arap_data.with_dynamics = true;
    arap_data.max_iter = (int)(iterations > 0 ? iterations : 100);
    Eigen::VectorXi b = Eigen::VectorXi::Zero(0);   // free boundary
    Eigen::MatrixXd bc = Eigen::MatrixXd::Zero(0, 0);

    if (!igl::arap_precomputation(V, F, 2, b, arap_data)) return RAPHOS_ERROR;
    if (!igl::arap_solve(bc, arap_data, UV)) return RAPHOS_ERROR;

    WriteUv(UV, uv, nuv);
    return RAPHOS_SUCCESS;
}
