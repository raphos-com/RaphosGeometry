#include "MarchingCubes.h"
#include "EigenMeshUtils.h"
#include <igl/marching_cubes.h>

// Extract an isosurface triangle mesh from a scalar field sampled on a regular
// nx*ny*nz grid (libigl marching_cubes). Pairs with Synera's voxel/field types.
// scalars: length ns = nx*ny*nz; gridVerts: ns interleaved xyz grid-point positions.
int MarchingCubes(
    double* scalars, Long ns,
    double* gridVerts,
    Long nx, Long ny, Long nz,
    double isovalue,
    double** oV, Long& onv,
    Long** oF, Long& onf
) {
    using namespace RaphosGeo;

    Eigen::VectorXd S(ns);
    for (Long i = 0; i < ns; i++) S(i) = scalars[i];

    Eigen::MatrixXd GV(ns, 3);
    for (Long i = 0; i < ns; i++)
    {
        GV(i, 0) = gridVerts[i * 3 + 0];
        GV(i, 1) = gridVerts[i * 3 + 1];
        GV(i, 2) = gridVerts[i * 3 + 2];
    }

    Eigen::MatrixXd V;
    Eigen::MatrixXi F;
    igl::marching_cubes(S, GV, (unsigned)nx, (unsigned)ny, (unsigned)nz, isovalue, V, F);

    WriteEigenVerts(V, oV, onv);
    WriteEigenFaces(F, oF, onf);
    return RAPHOS_SUCCESS;
}
