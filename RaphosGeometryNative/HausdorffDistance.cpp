#include "HausdorffDistance.h"
#include "GeoMeshUtils.h"
#include <geogram/basic/common.h>
#include <geogram/mesh/mesh_AABB.h>
#include <geogram/basic/geometry.h>
#include <cmath>

// Vertex-sampled (one-directional and symmetric) Hausdorff distance between two triangle meshes,
// using Geogram AABB trees for closest-point queries. Directed distance A->B is the maximum over
// A's vertices of the distance to the nearest point on B; symmetric is max(A->B, B->A).
// Note: this samples vertices (a lower bound on the true continuous Hausdorff distance) — adequate
// for bounded-deviation checks and much cheaper than dense surface sampling.
static double DirectedHausdorff(GEO::Mesh& from, GEO::MeshFacetsAABB& toAABB)
{
    using namespace GEO;
    double maxDist = 0.0;
    for (index_t i = 0; i < from.vertices.nb(); i++)
    {
        const double* p = from.vertices.point_ptr(i);
        vec3 q(p[0], p[1], p[2]);
        vec3 nearest;
        double sq = 0.0;
        toAABB.nearest_facet(q, nearest, sq);
        double d = std::sqrt(sq);
        if (d > maxDist) maxDist = d;
    }
    return maxDist;
}

int HausdorffDistance(
    double* va, Long nva, Long* fa, Long nfa,
    double* vb, Long nvb, Long* fb, Long nfb,
    double& dAB, double& dBA, double& dSym
) {
    using namespace GEO;
    initialize();

    Mesh A, B;
    RaphosGeo::BuildGeoMesh(va, nva, fa, nfa, A);
    RaphosGeo::BuildGeoMesh(vb, nvb, fb, nfb, B);

    MeshFacetsAABB aabbA, aabbB;
    aabbA.initialize(A);
    aabbB.initialize(B);

    dAB = DirectedHausdorff(A, aabbB);
    dBA = DirectedHausdorff(B, aabbA);
    dSym = dAB > dBA ? dAB : dBA;
    return RAPHOS_SUCCESS;
}
