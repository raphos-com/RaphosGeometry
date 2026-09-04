#include "CleanPointCloud.h"
#include <geogram/basic/common.h>
#include <geogram/basic/process.h>
#include <geogram/mesh/mesh.h>
#include <geogram/mesh/mesh_repair.h>
#include <geogram/points/nn_search.h>
#include <geogram/points/kd_tree.h>

// Remove outlier points: drop any point whose N-th nearest neighbour lies farther than radius r.
// Ported (cleaned) from the RaphosTools RemoveOutliers node. Uses the in-place buffer convention —
// the caller pre-allocates newPnts to the input capacity; we write the survivors and their count.
int CleanPointCloud(
    double* pnts, Long pntCount,
    double* newPnts, Long* newPntsCount,
    Long n, double r
) {
    using namespace GEO;
    initialize();

    index_t N = (index_t)(n > 0 ? n : 70);

    Mesh mesh_;
    mesh_.vertices.assign_points(pnts, 3, pntCount);
    mesh_repair(mesh_, GEO::MESH_REPAIR_COLOCATE, 0.0);

    NearestNeighborSearch_var NN = new BalancedKdTree(3);
    NN->set_points(mesh_.vertices.nb(), mesh_.vertices.point_ptr(0));

    vector<index_t> remove_point(mesh_.vertices.nb(), 0);
    double R2 = r * r;

    parallel_for_slice(
        0, mesh_.vertices.nb(),
        [&mesh_, N, &NN, R2, &remove_point](index_t from, index_t to) {
            vector<index_t> neigh(N);
            vector<double> neigh_sq_dist(N);
            for (index_t v = from; v < to; ++v) {
                NN->get_nearest_neighbors(
                    N, mesh_.vertices.point_ptr(v),
                    neigh.data(), neigh_sq_dist.data());
                remove_point[v] = (neigh_sq_dist[N - 1] > R2);
            }
        });

    mesh_.vertices.delete_elements(remove_point);

    newPntsCount[0] = (Long)mesh_.vertices.nb();
    for (index_t i : mesh_.vertices) {
        vec3 v = mesh_.vertices.point(i);
        newPnts[i * 3 + 0] = v.x;
        newPnts[i * 3 + 1] = v.y;
        newPnts[i * 3 + 2] = v.z;
    }
    return RAPHOS_SUCCESS;
}
