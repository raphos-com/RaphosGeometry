#include "VectorHeatTransport.h"
#include <vector>
#include <memory>
#include "geometrycentral/surface/vector_heat_method.h"
#include "geometrycentral/surface/surface_mesh_factories.h"

// Vector Heat Method (Sharp, Soliman & Crane 2019): parallel-transport a tangent vector defined at
// a source vertex across the whole surface. The per-vertex tangent result is expressed back in
// world space via each vertex's tangent basis.
int VectorHeatTransport(
    double* pnts, Long nv,
    size_t* faces, Long nf,
    size_t sourceIdx, double svx, double svy, double svz,
    double** vectors, Long& nvectors
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

    geometry->requireVertexTangentBasis();

    // Project the world-space source direction onto the source vertex's tangent basis.
    std::array<Vector3, 2> srcBasis = geometry->vertexTangentBasis[mesh->vertex(sourceIdx)];
    Vector3 sw{ svx, svy, svz };
    Vector2 srcTangent{ dot(sw, srcBasis[0]), dot(sw, srcBasis[1]) };

    VectorHeatMethodSolver solver(*geometry);
    VertexData<Vector2> transported =
        solver.transportTangentVector(mesh->vertex(sourceIdx), srcTangent);

    double* buf = new double[nv * 3];
    for (Long i = 0; i < nv; i++)
    {
        Vertex v = mesh->vertex(i);
        Vector2 t = transported[v];
        std::array<Vector3, 2> basis = geometry->vertexTangentBasis[v];
        Vector3 world = basis[0] * t.x + basis[1] * t.y;
        buf[i * 3 + 0] = world.x;
        buf[i * 3 + 1] = world.y;
        buf[i * 3 + 2] = world.z;
    }
    *vectors = buf;
    nvectors = nv;
    return RAPHOS_SUCCESS;
}
