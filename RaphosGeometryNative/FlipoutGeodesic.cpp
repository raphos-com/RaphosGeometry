#include "FlipoutGeodesic.h"
#include <vector>
#include <memory>
#include "geometrycentral/surface/flip_geodesics.h"
#include "geometrycentral/surface/surface_mesh_factories.h"

// FlipOut edge-flip geodesic PATH between two vertices (Geometry Central). Ported from the
// RaphosTools MeshGeodesic node — the single-path counterpart to the heat/exact distance fields.
// Returns the concatenated polyline points along the surface.
int FlipoutGeodesic(
    double* pnts, Long nv,
    size_t* faces, Long nf,
    size_t startIdx, size_t endIdx,
    double** newPnts, Long& newPntCount
) {
    using namespace geometrycentral;
    using namespace geometrycentral::surface;

    std::vector<Vector3> vertexPositions;
    vertexPositions.reserve(nv);
    for (Long i = 0; i < nv; i++)
        vertexPositions.push_back(Vector3{ pnts[i * 3 + 0], pnts[i * 3 + 1], pnts[i * 3 + 2] });

    std::vector<std::vector<size_t>> polygons;
    polygons.reserve(nf);
    for (Long i = 0; i < nf; i++)
        polygons.push_back(std::vector<size_t>{ faces[i * 3 + 0], faces[i * 3 + 1], faces[i * 3 + 2] });

    std::tuple<std::unique_ptr<ManifoldSurfaceMesh>, std::unique_ptr<VertexPositionGeometry>>&
        lvals = makeManifoldSurfaceMeshAndGeometry(polygons, vertexPositions);
    std::unique_ptr<ManifoldSurfaceMesh> mesh = std::move(std::get<0>(lvals));
    std::unique_ptr<VertexPositionGeometry> geometry = std::move(std::get<1>(lvals));

    Vertex vStart = mesh->vertex(startIdx);
    Vertex vEnd = mesh->vertex(endIdx);
    std::unique_ptr<FlipEdgeNetwork> edgeNetwork =
        FlipEdgeNetwork::constructFromDijkstraPath(*mesh, *geometry, vStart, vEnd);
    edgeNetwork->iterativeShorten();
    edgeNetwork->posGeom = geometry.get();

    std::vector<std::vector<Vector3>> polylines = edgeNetwork->getPathPolyline3D();

    size_t total = 0;
    for (const auto& p : polylines) total += p.size();
    double* buf = new double[total * 3];
    size_t c = 0;
    for (const auto& polyline : polylines)
        for (const Vector3& v : polyline)
        {
            buf[c * 3 + 0] = v[0];
            buf[c * 3 + 1] = v[1];
            buf[c * 3 + 2] = v[2];
            c++;
        }
    *newPnts = buf;
    newPntCount = (Long)total;
    return RAPHOS_SUCCESS;
}
