#pragma once
#include "utils.h"
#include <geogram/mesh/mesh.h>

// Shared helpers to move triangle meshes between the interleaved interop buffers and a GEO::Mesh.
namespace RaphosGeo
{
    // Build a GEO::Mesh (triangulated surface) from interleaved points + face-index triples.
    inline void BuildGeoMesh(
        const double* pnts, Long nv,
        const Long* faces, Long nf,
        GEO::Mesh& M)
    {
        M.clear();
        M.vertices.assign_points(const_cast<double*>(pnts), 3, nv);
        for (Long i = 0; i < nf; i++)
        {
            M.facets.create_triangle(
                (GEO::index_t)faces[i * 3 + 0],
                (GEO::index_t)faces[i * 3 + 1],
                (GEO::index_t)faces[i * 3 + 2]);
        }
    }

    // Extract a GEO::Mesh's vertices and (triangle) facets into new[] interop buffers.
    inline void ExtractGeoMesh(
        const GEO::Mesh& M,
        double** oV, Long& onv,
        Long** oF, Long& onf)
    {
        onv = (Long)M.vertices.nb();
        double* vbuf = new double[onv * 3];
        for (GEO::index_t i = 0; i < M.vertices.nb(); i++)
        {
            const double* p = M.vertices.point_ptr(i);
            vbuf[i * 3 + 0] = p[0];
            vbuf[i * 3 + 1] = p[1];
            vbuf[i * 3 + 2] = p[2];
        }
        *oV = vbuf;

        onf = (Long)M.facets.nb();
        Long* fbuf = new Long[onf * 3];
        for (GEO::index_t f = 0; f < M.facets.nb(); f++)
        {
            fbuf[f * 3 + 0] = (Long)M.facets.vertex(f, 0);
            fbuf[f * 3 + 1] = (Long)M.facets.vertex(f, 1);
            fbuf[f * 3 + 2] = (Long)M.facets.vertex(f, 2);
        }
        *oF = fbuf;
    }
}
