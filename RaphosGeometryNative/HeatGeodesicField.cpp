#include "HeatGeodesicField.h"
#include <vector>
#include <memory>
#include "geometrycentral/surface/heat_method_distance.h"
#include "geometrycentral/surface/surface_mesh_factories.h"

// Heat-method geodesic distance FIELD (Crane, Weischedel & Wardetzky 2013):
// returns geodesic distance from the source vertices to every vertex of the mesh.
// This differs from the single-path geodesic node — it is a whole-mesh scalar field.
int HeatGeodesicField(
    double* pnts, Long nv,
    size_t* faces, Long nf,
    size_t* sources, Long nsources,
    double** distances, Long& ndist
) {
    using namespace geometrycentral;
    using namespace geometrycentral::surface;

    std::vector<Vector3> vertexPositions;
    vertexPositions.reserve(nv);
    for (Long i = 0; i < nv; i++)
    {
        vertexPositions.push_back(Vector3{
            pnts[i * 3 + 0], pnts[i * 3 + 1], pnts[i * 3 + 2] });
    }
    std::vector<std::vector<size_t>> polygons;
    polygons.reserve(nf);
    for (Long i = 0; i < nf; i++)
    {
        polygons.push_back(std::vector<size_t>{
            faces[i * 3 + 0], faces[i * 3 + 1], faces[i * 3 + 2] });
    }

    std::tuple<std::unique_ptr<ManifoldSurfaceMesh>, std::unique_ptr<VertexPositionGeometry>>&
        lvals = makeManifoldSurfaceMeshAndGeometry(polygons, vertexPositions);
    std::unique_ptr<ManifoldSurfaceMesh> mesh = std::move(std::get<0>(lvals));
    std::unique_ptr<VertexPositionGeometry> geometry = std::move(std::get<1>(lvals));

    HeatMethodDistanceSolver solver(*geometry);

    std::vector<Vertex> sourceVerts;
    sourceVerts.reserve(nsources);
    for (Long i = 0; i < nsources; i++)
        sourceVerts.push_back(mesh->vertex(sources[i]));

    VertexData<double> dist = solver.computeDistance(sourceVerts);

    ndist = nv;
    double* buf = new double[nv];
    for (Long i = 0; i < nv; i++)
        buf[i] = dist[mesh->vertex(i)];
    *distances = buf;

    return RAPHOS_SUCCESS;
}
