#include "AdvancingFront.h"
#include <geogram/basic/common.h>
#include <geogram/points/nn_search.h>
#include <geogram/points/kd_tree.h>
#include <Eigen/Dense>
#include <vector>
#include <queue>
#include <set>
#include <map>
#include <cmath>

// Ball-pivoting surface reconstruction (Bernardini et al. 1999) — an advancing-front method.
// A ball of the given radius rolls over the point set: from a seed triangle, it pivots around each
// front edge to the next point, emitting triangles until the front is exhausted. Safety caps bound
// the work so degenerate inputs can never loop forever.
using V3 = Eigen::Vector3d;

namespace {
    // Center of the ball of radius r touching a,b,c on the side of the (a,b,c) normal.
    bool ballCenter(const V3& a, const V3& b, const V3& c, double r, V3& center) {
        V3 ab = b - a, ac = c - a;
        V3 n = ab.cross(ac);
        double n2 = n.squaredNorm();
        if (n2 < 1e-20) return false;
        // Circumcenter of the triangle.
        V3 cc = a + (ab.squaredNorm() * ac - ac.squaredNorm() * ab).cross(n) / (2.0 * n2);
        double rc2 = (cc - a).squaredNorm();
        double h2 = r * r - rc2;
        if (h2 < 0) return false;                 // ball radius too small to touch all three
        center = cc + n.normalized() * std::sqrt(h2);
        return true;
    }
}

int AdvancingFront(
    double* pnts, Long nv, double radius,
    Long** faces, Long& faceCount
) {
    using namespace GEO;
    initialize();

    std::vector<V3> P((size_t)nv);
    for (Long i = 0; i < nv; i++) P[i] = V3(pnts[i*3], pnts[i*3+1], pnts[i*3+2]);

    NearestNeighborSearch_var NN = new BalancedKdTree(3);
    NN->set_points((index_t)nv, pnts);

    // Auto radius from mean nearest-neighbour spacing if not supplied.
    double rho = radius;
    if (rho <= 0) {
        index_t K = (index_t)std::min<Long>(6, nv);
        std::vector<index_t> nb(K); std::vector<double> nd(K);
        double s = 0; Long c = 0;
        for (Long i = 0; i < nv; i++) {
            NN->get_nearest_neighbors(K, &pnts[i*3], nb.data(), nd.data());
            if (K > 1) { s += std::sqrt(nd[1]); c++; }
        }
        rho = c ? (s / c) * 1.5 : 1.0;
    }
    double search = rho * 2.0;

    auto neighbors = [&](Long i) {
        index_t K = (index_t)std::min<Long>(24, nv);
        std::vector<index_t> nb(K); std::vector<double> nd(K);
        NN->get_nearest_neighbors(K, &pnts[i*3], nb.data(), nd.data());
        std::vector<Long> out;
        for (index_t j = 0; j < K; j++) if (std::sqrt(nd[j]) <= search && nb[j] != (index_t)i) out.push_back(nb[j]);
        return out;
    };

    std::vector<Long> tris;
    std::set<std::pair<Long,Long>> frontEdges;         // directed edges awaiting a pivot
    std::set<std::pair<Long,Long>> usedEdges;          // undirected edges already in a triangle
    auto uedge = [](Long a, Long b) { return a < b ? std::make_pair(a,b) : std::make_pair(b,a); };

    // Find a seed triangle: a pair + third point admitting an empty ball.
    auto findBall = [&](Long a, Long b, Long c, V3& ctr) {
        if (!ballCenter(P[a], P[b], P[c], rho, ctr)) return false;
        // Empty-ball test against the neighbourhood.
        for (Long m : neighbors(a)) {
            if (m == a || m == b || m == c) continue;
            if ((P[m] - ctr).norm() < rho - 1e-9) return false;
        }
        return true;
    };

    bool seeded = false;
    V3 ctr;
    for (Long a = 0; a < nv && !seeded; a++) {
        std::vector<Long> na = neighbors(a);
        for (size_t x = 0; x < na.size() && !seeded; x++)
            for (size_t y = x + 1; y < na.size() && !seeded; y++) {
                Long b = na[x], c = na[y];
                if (findBall(a, b, c, ctr)) {
                    tris.push_back(a); tris.push_back(b); tris.push_back(c);
                    frontEdges.insert({a,b}); frontEdges.insert({b,c}); frontEdges.insert({c,a});
                    usedEdges.insert(uedge(a,b)); usedEdges.insert(uedge(b,c)); usedEdges.insert(uedge(c,a));
                    seeded = true;
                }
            }
    }

    long maxTris = (long)nv * 4 + 100;                  // safety cap
    while (!frontEdges.empty() && (long)(tris.size() / 3) < maxTris) {
        auto e = *frontEdges.begin(); frontEdges.erase(frontEdges.begin());
        Long a = e.first, b = e.second;
        if (usedEdges.count(uedge(a,b)) && (tris.size() > 3)) { /* still try to pivot outward */ }

        // Pivot: choose the candidate c minimizing the ball center distance (smallest pivot).
        Long bestC = -1; double bestKey = 1e30; V3 bestCtr;
        std::vector<Long> cand = neighbors(a);
        for (Long c : cand) {
            if (c == a || c == b) continue;
            if (usedEdges.count(uedge(a,c)) && usedEdges.count(uedge(b,c))) continue;
            V3 cc;
            if (!findBall(a, b, c, cc)) continue;
            double key = (cc - (P[a]+P[b])*0.5).norm();
            if (key < bestKey) { bestKey = key; bestC = c; bestCtr = cc; }
        }
        if (bestC < 0) continue;
        Long c = bestC;
        if (usedEdges.count(uedge(a,c)) && usedEdges.count(uedge(b,c)) && usedEdges.count(uedge(a,b))) continue;

        tris.push_back(a); tris.push_back(b); tris.push_back(c);
        usedEdges.insert(uedge(a,b)); usedEdges.insert(uedge(b,c)); usedEdges.insert(uedge(c,a));
        // Add the two new edges to the front if not already closed.
        if (!usedEdges.count(uedge(a,c)) || true) frontEdges.insert({a,c});
        frontEdges.insert({c,b});
    }

    faceCount = (Long)(tris.size() / 3);
    Long* buf = new Long[faceCount > 0 ? faceCount * 3 : 1];
    for (size_t i = 0; i < tris.size(); i++) buf[i] = tris[i];
    *faces = buf;
    return RAPHOS_SUCCESS;
}
