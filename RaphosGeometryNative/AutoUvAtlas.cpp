#include "AutoUvAtlas.h"
#include "GeoMeshUtils.h"
#include <geogram/basic/common.h>
#include <geogram/basic/command_line.h>
#include <geogram/basic/command_line_args.h>
#include <geogram/mesh/mesh.h>
#include <geogram/mesh/mesh_repair.h>
#include <geogram/parameterization/mesh_atlas_maker.h>
#include <vector>

// Segment a mesh into charts and flatten each (Geogram's atlas maker), producing per-face-corner
// UVs so seams are preserved without changing the geometry. Uses LSCM + a tetris packer to avoid
// the optional XATLAS dependency.
int AutoUvAtlas(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double hardAngle,
    double** uv, Long& nCornerUv
) {
    using namespace GEO;
    initialize();
    CmdLine::import_arg_group("standard");
    CmdLine::import_arg_group("algo");

    Mesh M;
    RaphosGeo::BuildGeoMesh(pnts, nv, faces, nf, M);
    mesh_repair(M, MESH_REPAIR_COLOCATE);

    mesh_make_atlas(
        M,
        hardAngle > 0.0 ? hardAngle : 45.0,
        PARAM_LSCM,
        PACK_TETRIS,
        false);

    Attribute<double> tex;
    tex.bind_if_is_defined(M.facet_corners.attributes(), "tex_coord");
    if (!tex.is_bound())
        return RAPHOS_ERROR;

    std::vector<double> out;
    out.reserve((size_t)M.facet_corners.nb() * 2);
    for (index_t f : M.facets)
        for (index_t c : M.facets.corners(f))
        {
            out.push_back(tex[2 * c + 0]);
            out.push_back(tex[2 * c + 1]);
        }

    nCornerUv = (Long)(out.size() / 2);
    double* buf = new double[out.size() > 0 ? out.size() : 1];
    for (size_t i = 0; i < out.size(); i++) buf[i] = out[i];
    *uv = buf;
    return RAPHOS_SUCCESS;
}
