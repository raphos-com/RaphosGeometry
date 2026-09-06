#include "MeshRepairOps.h"
#include "GeoMeshUtils.h"
#include <geogram/basic/common.h>
#include <geogram/basic/command_line.h>
#include <geogram/basic/command_line_args.h>
#include <geogram/mesh/mesh_repair.h>
#include <geogram/mesh/mesh_surface_intersection.h>

// Merge colocated vertices, drop duplicate/degenerate facets, optionally triangulate.
int RepairMesh(
    double* pnts, Long nv, Long* faces, Long nf,
    double colocateEpsilon, bool triangulate,
    double** oV, Long& onv, Long** oF, Long& onf
) {
    using namespace GEO;
    initialize();
    // Re-triangulation inside mesh_repair reads algorithm CmdLine variables; import the arg
    // groups that declare them, otherwise geogram asserts "variable_exists".
    CmdLine::import_arg_group("standard");
    CmdLine::import_arg_group("algo");

    Mesh M;
    RaphosGeo::BuildGeoMesh(pnts, nv, faces, nf, M);

    int mode = MESH_REPAIR_COLOCATE | MESH_REPAIR_DUP_F;
    if (triangulate) mode |= MESH_REPAIR_TRIANGULATE;
    mesh_repair(M, (MeshRepairMode)mode, colocateEpsilon);

    RaphosGeo::ExtractGeoMesh(M, oV, onv, oF, onf);
    return RAPHOS_SUCCESS;
}

// Coherently reorient facets (fix flipped triangles) using the Moebius rule.
int MakeConsistent(
    double* pnts, Long nv, Long* faces, Long nf,
    double** oV, Long& onv, Long** oF, Long& onf
) {
    using namespace GEO;
    initialize();

    Mesh M;
    RaphosGeo::BuildGeoMesh(pnts, nv, faces, nf, M);
    // reorient needs facet-facet adjacency; mesh_repair (colocate) initializes it.
    mesh_repair(M, MESH_REPAIR_COLOCATE);
    mesh_reorient(M);

    RaphosGeo::ExtractGeoMesh(M, oV, onv, oF, onf);
    return RAPHOS_SUCCESS;
}

// Resolve self-intersections into a clean, intersection-free triangulation (exact arithmetic).
int RemoveSelfIntersections(
    double* pnts, Long nv, Long* faces, Long nf,
    Long maxIter,
    double** oV, Long& onv, Long** oF, Long& onf
) {
    using namespace GEO;
    initialize();

    Mesh M;
    RaphosGeo::BuildGeoMesh(pnts, nv, faces, nf, M);
    mesh_remove_intersections(M, maxIter > 0 ? (index_t)maxIter : 3);

    RaphosGeo::ExtractGeoMesh(M, oV, onv, oF, onf);
    return RAPHOS_SUCCESS;
}
