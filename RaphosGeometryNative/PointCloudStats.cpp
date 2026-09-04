#include "PointCloudStats.h"
#include <geogram/basic/common.h>
#include <geogram/points/nn_search.h>
#include <geogram/points/kd_tree.h>
#include <map>
#include <tuple>
#include <cmath>
#include <vector>

// Mean nearest-neighbour spacing: for each point, average the distance to its k nearest
// neighbours (excluding itself), then average over all points (Geogram kd-tree).
int AverageSpacing(double* pnts, Long nv, Long k, double& spacing)
{
    using namespace GEO;
    initialize();

    index_t K = (index_t)(k >= 1 ? k : 6) + 1;   // +1 because the point itself is its 0-th neighbour
    if ((Long)K > nv) K = (index_t)nv;

    NearestNeighborSearch_var NN = new BalancedKdTree(3);
    NN->set_points((index_t)nv, pnts);

    std::vector<index_t> neigh(K);
    std::vector<double> nsq(K);
    double total = 0.0;
    Long counted = 0;
    for (Long i = 0; i < nv; i++)
    {
        NN->get_nearest_neighbors(K, &pnts[i * 3], neigh.data(), nsq.data());
        double s = 0.0; int m = 0;
        for (index_t j = 1; j < K; j++) { s += std::sqrt(nsq[j]); m++; }   // skip self at j==0
        if (m > 0) { total += s / m; counted++; }
    }
    spacing = counted > 0 ? total / counted : 0.0;
    return RAPHOS_SUCCESS;
}

// Voxel-grid downsampling: bucket points into cubic cells of side `cell` and emit one centroid
// per occupied cell. Deterministic and order-independent.
int SimplifyPointCloud(
    double* pnts, Long nv, double cell,
    double** outPnts, Long& outCount
) {
    if (cell <= 0.0) cell = 1.0;

    std::map<std::tuple<long long, long long, long long>, std::tuple<double, double, double, long long>> cells;
    for (Long i = 0; i < nv; i++)
    {
        double x = pnts[i * 3 + 0], y = pnts[i * 3 + 1], z = pnts[i * 3 + 2];
        auto key = std::make_tuple(
            (long long)std::floor(x / cell),
            (long long)std::floor(y / cell),
            (long long)std::floor(z / cell));
        auto& acc = cells[key];
        std::get<0>(acc) += x;
        std::get<1>(acc) += y;
        std::get<2>(acc) += z;
        std::get<3>(acc) += 1;
    }

    outCount = (Long)cells.size();
    double* buf = new double[outCount * 3];
    Long idx = 0;
    for (const auto& kv : cells)
    {
        double n = (double)std::get<3>(kv.second);
        buf[idx * 3 + 0] = std::get<0>(kv.second) / n;
        buf[idx * 3 + 1] = std::get<1>(kv.second) / n;
        buf[idx * 3 + 2] = std::get<2>(kv.second) / n;
        idx++;
    }
    *outPnts = buf;
    return RAPHOS_SUCCESS;
}
