#include "MeshFromPointCloud.h"
#include <geogram/basic/numeric.h>
#include <geogram/basic/command_line.h>
#include <geogram/basic/command_line_args.h>
#include <geogram/mesh/mesh.h>
#include <geogram/points/co3ne.h>
#include <geogram/mesh/mesh_geometry.h>

// Co3Ne point-cloud reconstruction (Geogram). Ported from the RaphosTools node.
// https://github.com/BrunoLevy/geogram/wiki/Reconstruction

static void add_normals(GEO::Mesh& M, double* normals, Long pntCount)
{
    using namespace GEO;
    Attribute<double> normal;
    normal.bind_if_is_defined(M.vertices.attributes(), "normal");
    if (!normal.is_bound())
        normal.create_vector_attribute(M.vertices.attributes(), "normal", 3);
    std::copy(normals, normals + pntCount * 3, normal.data());
}

int MeshFromPointCloud(
    double* pnts, double* normals, Long pntCount,
    MeshFromPointCloudConfig cfg,
    double** newPnts, Long& newPntsLength,
    Long** faces, Long& faceCount
) {
    using namespace GEO;
    initialize();
    CmdLine::import_arg_group("co3ne");
    CmdLine::import_arg_group("algo");

    CmdLine::set_arg("co3ne:nb_neighbors", cfg.nb_neighbors);
    CmdLine::set_arg("co3ne:Psmooth_iter", cfg.Psmooth_iter);
    CmdLine::set_arg("co3ne:radius", cfg.radius);
    CmdLine::set_arg("co3ne:repair", cfg.repair);
    CmdLine::set_arg("co3ne:max_N_angle", cfg.max_N_angle);
    CmdLine::set_arg("co3ne:max_hole_area", cfg.max_hole_area);
    CmdLine::set_arg("co3ne:max_hole_edges", cfg.max_hole_edges);
    CmdLine::set_arg("co3ne:min_comp_area", cfg.min_comp_area);
    CmdLine::set_arg("co3ne:min_comp_facets", cfg.min_comp_facets);

    Mesh mesh;
    mesh.vertices.assign_points(pnts, 3, pntCount);
    if (normals)
        add_normals(mesh, normals, pntCount);

    Co3Ne_reconstruct(mesh, cfg.radius);

    faceCount = (Long)mesh.facets.nb();
    Long* faceBuf = new Long[faceCount * 3];
    for (index_t f : mesh.facets) {
        faceBuf[f * 3 + 0] = (Long)mesh.facets.vertex(f, 0);
        faceBuf[f * 3 + 1] = (Long)mesh.facets.vertex(f, 1);
        faceBuf[f * 3 + 2] = (Long)mesh.facets.vertex(f, 2);
    }
    *faces = faceBuf;

    newPntsLength = (Long)mesh.vertices.nb();
    double* vBuf = new double[newPntsLength * 3];
    for (index_t i : mesh.vertices) {
        vec3 v = mesh.vertices.point(i);
        vBuf[i * 3 + 0] = v.x;
        vBuf[i * 3 + 1] = v.y;
        vBuf[i * 3 + 2] = v.z;
    }
    *newPnts = vBuf;

    return RAPHOS_SUCCESS;
}
