#include "PoissonReconstruct.h"
#include "GeoMeshUtils.h"
#include <geogram/basic/common.h>
#include <geogram/mesh/mesh.h>
#include <geogram/third_party/PoissonRecon/poisson_geogram.h>
#include <algorithm>

// Screened Poisson surface reconstruction (Kazhdan) via Geogram's bundled PoissonRecon wrapper.
// The input point set must carry per-point normals; the result is a watertight surface.
int PoissonReconstruct(
    double* pnts, double* normals, Long nv,
    Long depth,
    double** oV, Long& onv,
    Long** oF, Long& onf
) {
    using namespace GEO;
    initialize();

    Mesh points;
    points.vertices.assign_points(pnts, 3, nv);

    // Attach the required "normal" vector attribute (dimension 3) to the vertices.
    Attribute<double> normal;
    normal.create_vector_attribute(points.vertices.attributes(), "normal", 3);
    std::copy(normals, normals + nv * 3, normal.data());

    Mesh surface;
    PoissonReconstruction pr;
    pr.set_depth((index_t)(depth > 0 ? depth : 8));
    pr.reconstruct(&points, &surface);

    RaphosGeo::ExtractGeoMesh(surface, oV, onv, oF, onf);
    return RAPHOS_SUCCESS;
}
