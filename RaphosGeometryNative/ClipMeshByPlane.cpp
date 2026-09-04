#include "ClipMeshByPlane.h"
#include <vector>
#include <map>
#include <array>
#include <cmath>

// Clip a triangle mesh against a plane, keeping the half where dot(v - p, n) <= 0.
// Triangles fully on the kept side are copied; straddling triangles are split at the plane
// (Sutherland-Hodgman against a half-space) and re-triangulated. New boundary vertices are welded
// via an edge cache so the cut edge stays watertight along shared edges.
int ClipMeshByPlane(
    double* pnts, Long nv,
    Long* faces, Long nf,
    double px, double py, double pz,
    double nx, double ny, double nz,
    double** oV, Long& onv,
    Long** oF, Long& onf
) {
    // Normalize the plane normal.
    double nlen = std::sqrt(nx * nx + ny * ny + nz * nz);
    if (nlen == 0.0) return RAPHOS_ERROR;
    nx /= nlen; ny /= nlen; nz /= nlen;

    auto dist = [&](Long v) {
        return (pnts[v * 3 + 0] - px) * nx + (pnts[v * 3 + 1] - py) * ny + (pnts[v * 3 + 2] - pz) * nz;
    };

    std::vector<double> outV;
    std::vector<Long> outF;
    std::map<Long, Long> origMap;                       // original vertex index -> new index
    std::map<std::pair<Long, Long>, Long> edgeMap;      // undirected edge -> new (intersection) index

    auto addOrig = [&](Long v) -> Long {
        auto it = origMap.find(v);
        if (it != origMap.end()) return it->second;
        Long idx = (Long)(outV.size() / 3);
        outV.push_back(pnts[v * 3 + 0]); outV.push_back(pnts[v * 3 + 1]); outV.push_back(pnts[v * 3 + 2]);
        origMap[v] = idx;
        return idx;
    };
    auto addEdge = [&](Long a, Long b, double da, double db) -> Long {
        std::pair<Long, Long> key = a < b ? std::make_pair(a, b) : std::make_pair(b, a);
        auto it = edgeMap.find(key);
        if (it != edgeMap.end()) return it->second;
        double t = da / (da - db);                      // interpolation to the zero crossing
        Long idx = (Long)(outV.size() / 3);
        for (int c = 0; c < 3; c++)
            outV.push_back(pnts[a * 3 + c] + t * (pnts[b * 3 + c] - pnts[a * 3 + c]));
        edgeMap[key] = idx;
        return idx;
    };

    for (Long f = 0; f < nf; f++)
    {
        Long v[3] = { faces[f * 3 + 0], faces[f * 3 + 1], faces[f * 3 + 2] };
        double d[3] = { dist(v[0]), dist(v[1]), dist(v[2]) };

        // Sutherland-Hodgman clip of the triangle against the half-space d <= 0.
        std::vector<Long> poly;   // new-vertex indices of the kept polygon
        for (int i = 0; i < 3; i++)
        {
            int j = (i + 1) % 3;
            bool ai = d[i] <= 0.0, bi = d[j] <= 0.0;
            if (ai) poly.push_back(addOrig(v[i]));
            if (ai != bi) poly.push_back(addEdge(v[i], v[j], d[i], d[j]));
        }
        // Fan-triangulate the kept polygon (3 or 4 vertices).
        for (size_t i = 1; i + 1 < poly.size(); i++)
        {
            outF.push_back(poly[0]); outF.push_back(poly[i]); outF.push_back(poly[i + 1]);
        }
    }

    onv = (Long)(outV.size() / 3);
    double* vbuf = new double[outV.size() > 0 ? outV.size() : 1];
    for (size_t i = 0; i < outV.size(); i++) vbuf[i] = outV[i];
    *oV = vbuf;

    onf = (Long)(outF.size() / 3);
    Long* fbuf = new Long[outF.size() > 0 ? outF.size() : 1];
    for (size_t i = 0; i < outF.size(); i++) fbuf[i] = outF[i];
    *oF = fbuf;

    return RAPHOS_SUCCESS;
}
