#include "OrientNormals.h"
#include <geogram/basic/common.h>
#include <geogram/points/nn_search.h>
#include <geogram/points/kd_tree.h>
#include <vector>
#include <queue>
#include <tuple>
#include <functional>
#include <cmath>

// Consistently orient point-cloud normals by propagating sign along a minimum spanning tree of the
// k-nearest-neighbour graph, where edge weight = 1 - |n_i . n_j| (Hoppe et al. 1992). CGAL's
// mst_orient_normals is GPL, so this is a clean from-scratch reimplementation.
int OrientNormals(
    double* pnts, Long nv,
    double* normals, Long k,
    double** outNormals, Long& nout
) {
    using namespace GEO;
    initialize();

    index_t K = (index_t)(k >= 2 ? k : 12);
    if ((Long)K > nv) K = (index_t)nv;

    // Working copy of the normals we will flip in place.
    std::vector<double> n(normals, normals + nv * 3);

    NearestNeighborSearch_var NN = new BalancedKdTree(3);
    NN->set_points((index_t)nv, pnts);

    // Symmetric k-NN adjacency.
    std::vector<std::vector<index_t>> adj((size_t)nv);
    {
        std::vector<index_t> neigh(K);
        std::vector<double> nsq(K);
        for (Long i = 0; i < nv; i++)
        {
            NN->get_nearest_neighbors(K, &pnts[i * 3], neigh.data(), nsq.data());
            for (index_t j = 0; j < K; j++)
            {
                index_t p = neigh[j];
                if (p != (index_t)i) { adj[i].push_back(p); adj[p].push_back((index_t)i); }
            }
        }
    }

    auto dot = [&](index_t a, index_t b) {
        return n[a * 3 + 0] * n[b * 3 + 0] + n[a * 3 + 1] * n[b * 3 + 1] + n[a * 3 + 2] * n[b * 3 + 2];
    };
    auto flip = [&](index_t a) {
        n[a * 3 + 0] = -n[a * 3 + 0]; n[a * 3 + 1] = -n[a * 3 + 1]; n[a * 3 + 2] = -n[a * 3 + 2];
    };

    // Seed: the point with the largest Z, oriented to point +Z (outward for typical shapes).
    index_t seed = 0;
    for (Long i = 1; i < nv; i++) if (pnts[i * 3 + 2] > pnts[seed * 3 + 2]) seed = (index_t)i;
    if (n[seed * 3 + 2] < 0.0) flip(seed);

    // Prim's MST from the seed; align each newly-reached normal with the parent it came from.
    std::vector<char> visited((size_t)nv, 0);
    // priority queue of (weight, parent, child): smallest weight first.
    typedef std::tuple<double, index_t, index_t> Edge;
    std::priority_queue<Edge, std::vector<Edge>, std::greater<Edge>> pq;

    visited[seed] = 1;
    for (index_t nb : adj[seed]) pq.push(std::make_tuple(1.0 - std::fabs(dot(seed, nb)), seed, nb));

    while (!pq.empty())
    {
        Edge e = pq.top(); pq.pop();
        index_t u = std::get<1>(e), v = std::get<2>(e);
        if (visited[v]) continue;
        visited[v] = 1;
        if (dot(u, v) < 0.0) flip(v);
        for (index_t nb : adj[v])
            if (!visited[nb]) pq.push(std::make_tuple(1.0 - std::fabs(dot(v, nb)), v, nb));
    }

    nout = nv;
    double* buf = new double[nv * 3];
    for (Long i = 0; i < nv * 3; i++) buf[i] = n[i];
    *outNormals = buf;
    return RAPHOS_SUCCESS;
}
