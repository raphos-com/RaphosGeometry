#include "FillHoles.h"
#include <geogram/basic/common.h>
#include <geogram/basic/command_line.h>
#include <geogram/basic/command_line_args.h>
#include <geogram/mesh/mesh.h>
#include <geogram/mesh/mesh_fill_holes.h>
#include <geogram/mesh/mesh_repair.h>
#include <vector>

// Fill holes (boundary loops) in a triangle mesh using Geogram's fill_holes.
// maxHoleArea = 0 fills all holes; maxHoleEdges caps the boundary length of a hole
// that will be filled.
int FillHoles(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double maxHoleArea, Long maxHoleEdges,
    double** oV, Long& onv,
    Long** oF, Long& onf
) {
    using namespace GEO;
    initialize();
    // Declares algo:hole_filling (read by fill_holes) and related algorithm options.
    CmdLine::import_arg_group("standard");
    CmdLine::import_arg_group("algo");

    Mesh M;
    M.vertices.assign_points(pnts, 3, nv);
    for (Long i = 0; i < nf; i++)
    {
        M.facets.create_triangle(
            (index_t)faces[i * 3 + 0],
            (index_t)faces[i * 3 + 1],
            (index_t)faces[i * 3 + 2]);
    }
    // Build facet adjacency so border edges (hole boundaries) can be found.
    mesh_repair(M, MESH_REPAIR_DEFAULT);

    // Geogram's fill_holes treats max_area == 0 (or max_edges == 0) as "do nothing".
    // Our node contract is that 0 means "fill everything", so map 0 -> effectively unbounded.
    double area = maxHoleArea > 0.0 ? maxHoleArea : 1e30;
    index_t maxEdges = maxHoleEdges > 0 ? (index_t)maxHoleEdges : max_index_t();
    fill_holes(M, area, maxEdges, true);

    onv = (Long)M.vertices.nb();
    double* vbuf = new double[onv * 3];
    for (index_t i = 0; i < M.vertices.nb(); i++)
    {
        const double* p = M.vertices.point_ptr(i);
        vbuf[i * 3 + 0] = p[0];
        vbuf[i * 3 + 1] = p[1];
        vbuf[i * 3 + 2] = p[2];
    }
    *oV = vbuf;

    onf = (Long)M.facets.nb();
    Long* fbuf = new Long[onf * 3];
    for (index_t f = 0; f < M.facets.nb(); f++)
    {
        fbuf[f * 3 + 0] = (Long)M.facets.vertex(f, 0);
        fbuf[f * 3 + 1] = (Long)M.facets.vertex(f, 1);
        fbuf[f * 3 + 2] = (Long)M.facets.vertex(f, 2);
    }
    *oF = fbuf;

    return RAPHOS_SUCCESS;
}
