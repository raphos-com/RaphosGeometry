#include "FillHoles.h"
#include <geogram/basic/common.h>
#include <geogram/basic/command_line.h>
#include <geogram/basic/command_line_args.h>
#include <geogram/mesh/mesh.h>
#include <geogram/mesh/mesh_fill_holes.h>
#include <geogram/mesh/mesh_repair.h>
#include <functional>
#include <map>
#include <utility>
#include <vector>

// Fill holes (boundary loops) in a triangle mesh using Geogram's fill_holes.
// maxHoleArea = 0 fills all holes; maxHoleEdges caps the boundary length of a hole
// that will be filled.
//
// In addition to the repaired mesh, the node reports the patch that was generated for
// each hole as its own little mesh, so the example can highlight exactly what was added.
// Patches are identified by tagging every pre-existing facet before fill_holes runs; any
// facet left untagged afterwards is new. The new facets are then split into connected
// components (one per hole) via union-find over their shared edges.
int FillHoles(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double maxHoleArea, Long maxHoleEdges,
    double** oV, Long& onv,
    Long** oF, Long& onf,
    Long& onPatch,
    Long** oPatchVCount, Long** oPatchFCount,
    double** oPV, Long& oPVtotal,
    Long** oPF, Long& oPFtotal
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
    // Build facet adjacency so border edges (hole boundaries) can be found. This also
    // colocates/compacts the input, so from here the facet numbering is stable.
    mesh_repair(M, MESH_REPAIR_DEFAULT);

    // Snapshot the facet count: fill_holes only appends patch facets, so everything at
    // index >= nfBefore is new. We pass repair=false so this range stays valid (a final
    // repair would renumber facets); the patch triangles already reuse the loop's existing
    // vertices, so the face set is watertight without it.
    const index_t nfBefore = M.facets.nb();

    // Geogram's fill_holes treats max_area == 0 (or max_edges == 0) as "do nothing".
    // Our node contract is that 0 means "fill everything", so map 0 -> effectively unbounded.
    double area = maxHoleArea > 0.0 ? maxHoleArea : 1e30;
    index_t maxEdges = maxHoleEdges > 0 ? (index_t)maxHoleEdges : max_index_t();
    fill_holes(M, area, maxEdges, false);

    // ---- full result mesh ----
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

    // ---- per-hole patch meshes ----
    // Facets appended by fill_holes (index >= nfBefore) are the new patch triangles.
    std::vector<index_t> patchFacets;
    for (index_t f = nfBefore; f < M.facets.nb(); f++)
        patchFacets.push_back(f);

    // Union-find over patch facets, joined when they share an edge (an unordered vertex
    // pair). Facets of one hole's fan form one connected component.
    const index_t P = (index_t)patchFacets.size();
    std::vector<index_t> parent(P);
    for (index_t i = 0; i < P; i++) parent[i] = i;
    std::function<index_t(index_t)> findRoot = [&](index_t x) {
        while (parent[x] != x) { parent[x] = parent[parent[x]]; x = parent[x]; }
        return x;
    };
    auto unite = [&](index_t a, index_t b) {
        index_t ra = findRoot(a), rb = findRoot(b);
        if (ra != rb) parent[ra] = rb;
    };
    std::map<std::pair<index_t, index_t>, index_t> edgeOwner; // edge -> local patch index
    for (index_t li = 0; li < P; li++)
    {
        index_t f = patchFacets[li];
        for (index_t c = 0; c < 3; c++)
        {
            index_t a = (index_t)M.facets.vertex(f, c);
            index_t b = (index_t)M.facets.vertex(f, (c + 1) % 3);
            std::pair<index_t, index_t> e(a < b ? a : b, a < b ? b : a);
            auto it = edgeOwner.find(e);
            if (it == edgeOwner.end()) edgeOwner[e] = li;
            else unite(li, it->second);
        }
    }

    // Group local patch indices by their component root, preserving first-seen order.
    std::map<index_t, index_t> rootToGroup; // root -> group id
    std::vector<std::vector<index_t>> groups; // group id -> local patch indices
    for (index_t li = 0; li < P; li++)
    {
        index_t r = findRoot(li);
        auto it = rootToGroup.find(r);
        if (it == rootToGroup.end()) { rootToGroup[r] = (index_t)groups.size(); groups.push_back({}); }
        groups[rootToGroup[r]].push_back(li);
    }

    onPatch = (Long)groups.size();
    const Long allocPatch = onPatch > 0 ? onPatch : 1; // never new[0]
    Long* pvc = new Long[allocPatch];
    Long* pfc = new Long[allocPatch];
    std::vector<double> pv;   // concatenated patch vertices (compact per patch)
    std::vector<Long> pf;     // concatenated patch faces (local indices per patch)
    for (index_t g = 0; g < (index_t)groups.size(); g++)
    {
        // Compact the vertices this group references.
        std::map<index_t, Long> remap; // global vertex -> local index within this patch
        Long localFaceCount = 0;
        for (index_t li : groups[g])
        {
            index_t f = patchFacets[li];
            for (index_t c = 0; c < 3; c++)
            {
                index_t v = (index_t)M.facets.vertex(f, c);
                if (remap.find(v) == remap.end())
                {
                    Long local = (Long)remap.size();
                    remap[v] = local;
                    const double* p = M.vertices.point_ptr(v);
                    pv.push_back(p[0]); pv.push_back(p[1]); pv.push_back(p[2]);
                }
            }
            pf.push_back(remap[(index_t)M.facets.vertex(f, 0)]);
            pf.push_back(remap[(index_t)M.facets.vertex(f, 1)]);
            pf.push_back(remap[(index_t)M.facets.vertex(f, 2)]);
            localFaceCount++;
        }
        pvc[g] = (Long)remap.size();
        pfc[g] = localFaceCount;
    }

    *oPatchVCount = pvc;
    *oPatchFCount = pfc;

    oPVtotal = (Long)(pv.size() / 3);
    double* pvbuf = new double[pv.size() > 0 ? pv.size() : 1];
    for (size_t i = 0; i < pv.size(); i++) pvbuf[i] = pv[i];
    *oPV = pvbuf;

    oPFtotal = (Long)(pf.size() / 3);
    Long* pfbuf = new Long[pf.size() > 0 ? pf.size() : 1];
    for (size_t i = 0; i < pf.size(); i++) pfbuf[i] = pf[i];
    *oPF = pfbuf;

    return RAPHOS_SUCCESS;
}
