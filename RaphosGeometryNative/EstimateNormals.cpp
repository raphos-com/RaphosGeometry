#include "EstimateNormals.h"
#include <geogram/basic/common.h>
#include <geogram/points/nn_search.h>
#include <geogram/points/kd_tree.h>
#include <Eigen/Dense>
#include <vector>

// Estimate a unit normal at each point by PCA of its k nearest neighbours: the normal is the
// eigenvector of the local covariance matrix with the smallest eigenvalue. Sign is not
// disambiguated here (that is the job of a separate normal-orientation node).
int EstimateNormals(
    double* pnts, Long nv,
    Long k,
    double** normals, Long& nnormals
) {
    using namespace GEO;
    initialize();

    index_t K = (index_t)(k >= 3 ? k : 16);
    if ((Long)K > nv) K = (index_t)nv;

    NearestNeighborSearch_var NN = new BalancedKdTree(3);
    NN->set_points((index_t)nv, pnts);

    double* nbuf = new double[nv * 3];

    std::vector<index_t> neigh(K);
    std::vector<double> nsq(K);
    for (Long i = 0; i < nv; i++)
    {
        NN->get_nearest_neighbors(K, &pnts[i * 3], neigh.data(), nsq.data());

        // Centroid of the neighbourhood.
        Eigen::Vector3d c(0, 0, 0);
        for (index_t j = 0; j < K; j++)
        {
            index_t p = neigh[j];
            c += Eigen::Vector3d(pnts[p * 3 + 0], pnts[p * 3 + 1], pnts[p * 3 + 2]);
        }
        c /= (double)K;

        // Covariance matrix.
        Eigen::Matrix3d cov = Eigen::Matrix3d::Zero();
        for (index_t j = 0; j < K; j++)
        {
            index_t p = neigh[j];
            Eigen::Vector3d d(pnts[p * 3 + 0] - c.x(), pnts[p * 3 + 1] - c.y(), pnts[p * 3 + 2] - c.z());
            cov += d * d.transpose();
        }

        Eigen::SelfAdjointEigenSolver<Eigen::Matrix3d> es(cov);
        Eigen::Vector3d n = es.eigenvectors().col(0);   // smallest eigenvalue -> surface normal
        n.normalize();

        nbuf[i * 3 + 0] = n.x();
        nbuf[i * 3 + 1] = n.y();
        nbuf[i * 3 + 2] = n.z();
    }

    *normals = nbuf;
    nnormals = nv;
    return RAPHOS_SUCCESS;
}
