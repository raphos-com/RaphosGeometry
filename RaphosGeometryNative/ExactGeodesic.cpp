#include "ExactGeodesic.h"
#include "EigenMeshUtils.h"
#include <igl/exact_geodesic.h>

// Exact polyhedral geodesic distance (MMP algorithm) from source vertices to every mesh vertex.
// This is the exact counterpart to the heat-method field node.
int ExactGeodesic(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long* sources, Long nsources,
    double** distances, Long& ndist
) {
    using namespace RaphosGeo;

    Eigen::MatrixXd V;
    Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::VectorXi VS(nsources);
    for (Long i = 0; i < nsources; i++) VS(i) = (int)sources[i];
    Eigen::VectorXi FS, FT;                 // no source/target faces
    Eigen::VectorXi VT(nv);                 // targets = all vertices
    for (Long i = 0; i < nv; i++) VT(i) = (int)i;

    Eigen::VectorXd D;
    igl::exact_geodesic(V, F, VS, FS, VT, FT, D);

    WriteDoubles(D.data(), (Long)D.size(), distances, ndist);
    return RAPHOS_SUCCESS;
}
