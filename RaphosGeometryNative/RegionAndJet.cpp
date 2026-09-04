#include "RegionAndJet.h"
#include <geogram/basic/common.h>
#include <geogram/points/nn_search.h>
#include <geogram/points/kd_tree.h>
#include <Eigen/Dense>
#include <vector>
#include <queue>
#include <cmath>

using V3 = Eigen::Vector3d;

namespace {
    // Per-point normals + k-NN adjacency, shared by both algorithms.
    void buildNormalsAndAdj(double* pnts, Long nv, GEO::index_t K,
        std::vector<V3>& P, std::vector<V3>& N, std::vector<std::vector<GEO::index_t>>& adj)
    {
        using namespace GEO;
        P.resize((size_t)nv); N.resize((size_t)nv); adj.assign((size_t)nv, {});
        for (Long i = 0; i < nv; i++) P[i] = V3(pnts[i*3], pnts[i*3+1], pnts[i*3+2]);
        NearestNeighborSearch_var NN = new BalancedKdTree(3);
        NN->set_points((index_t)nv, pnts);
        std::vector<index_t> nb(K); std::vector<double> nd(K);
        for (Long i = 0; i < nv; i++) {
            NN->get_nearest_neighbors(K, &pnts[i*3], nb.data(), nd.data());
            V3 c = V3::Zero();
            for (index_t j = 0; j < K; j++) c += P[nb[j]];
            c /= (double)K;
            Eigen::Matrix3d cov = Eigen::Matrix3d::Zero();
            for (index_t j = 0; j < K; j++) { V3 d = P[nb[j]] - c; cov += d * d.transpose(); }
            Eigen::SelfAdjointEigenSolver<Eigen::Matrix3d> es(cov);
            N[i] = es.eigenvectors().col(0).normalized();
            for (index_t j = 0; j < K; j++) if (nb[j] != (index_t)i) adj[i].push_back(nb[j]);
        }
    }
}

int RegionGrowing(
    double* pnts, Long nv,
    double angleDeg, Long k, Long minRegion,
    Long** labels, Long& nLabels, Long& nRegions
) {
    using namespace GEO;
    initialize();
    index_t K = (index_t)std::min<Long>(k > 2 ? k : 12, nv);

    std::vector<V3> P, N; std::vector<std::vector<index_t>> adj;
    buildNormalsAndAdj(pnts, nv, K, P, N, adj);

    double cosTol = std::cos((angleDeg > 0 ? angleDeg : 15.0) * M_PI / 180.0);
    std::vector<Long> label((size_t)nv, -1);
    Long region = 0;

    for (Long s = 0; s < nv; s++) {
        if (label[s] != -1) continue;
        // BFS grow a region of normal-consistent points from seed s.
        std::vector<Long> members;
        std::queue<Long> q; q.push(s); label[s] = region; members.push_back(s);
        while (!q.empty()) {
            Long u = q.front(); q.pop();
            for (index_t vn : adj[u]) {
                if (label[vn] != -1) continue;
                if (std::fabs(N[u].dot(N[vn])) > cosTol) { label[vn] = region; members.push_back(vn); q.push(vn); }
            }
        }
        if ((Long)members.size() < (minRegion > 1 ? minRegion : 1)) {
            for (Long m : members) label[m] = -1;   // too small: discard
        } else {
            region++;
        }
    }

    nRegions = region;
    nLabels = nv;
    Long* buf = new Long[nv > 0 ? nv : 1];
    for (Long i = 0; i < nv; i++) buf[i] = label[i];
    *labels = buf;
    return RAPHOS_SUCCESS;
}

int JetCurvature(
    double* pnts, Long nv, Long k,
    double** k1, double** k2, Long& nout
) {
    using namespace GEO;
    initialize();
    index_t K = (index_t)std::min<Long>(k > 6 ? k : 18, nv);

    std::vector<V3> P, N; std::vector<std::vector<index_t>> adj;
    buildNormalsAndAdj(pnts, nv, K, P, N, adj);

    NearestNeighborSearch_var NN = new BalancedKdTree(3);
    NN->set_points((index_t)nv, pnts);

    double* K1 = new double[nv > 0 ? nv : 1];
    double* K2 = new double[nv > 0 ? nv : 1];

    std::vector<index_t> nb(K); std::vector<double> nd(K);
    for (Long i = 0; i < nv; i++) {
        NN->get_nearest_neighbors(K, &pnts[i*3], nb.data(), nd.data());
        // Local frame: z = normal, x/y span the tangent plane.
        V3 n = N[i];
        V3 t = (std::fabs(n.x()) < 0.9 ? V3(1,0,0) : V3(0,1,0));
        V3 ex = (t - n * t.dot(n)).normalized();
        V3 ey = n.cross(ex);
        // Fit z = a x^2 + b xy + c y^2 (quadratic height field, linear terms ~0 at the point).
        Eigen::MatrixXd A((int)K, 3); Eigen::VectorXd z((int)K);
        for (index_t j = 0; j < K; j++) {
            V3 d = P[nb[j]] - P[i];
            double x = d.dot(ex), y = d.dot(ey), zz = d.dot(n);
            A(j,0) = x*x; A(j,1) = x*y; A(j,2) = y*y; z(j) = zz;
        }
        Eigen::Vector3d abc = A.colPivHouseholderQr().solve(z);
        // Shape operator (Hessian of height field): [[2a, b],[b, 2c]]; eigenvalues = principal curvatures.
        Eigen::Matrix2d H; H << 2*abc(0), abc(1), abc(1), 2*abc(2);
        Eigen::SelfAdjointEigenSolver<Eigen::Matrix2d> es(H);
        double e0 = es.eigenvalues()(0), e1 = es.eigenvalues()(1);
        // k1 = larger magnitude.
        if (std::fabs(e0) >= std::fabs(e1)) { K1[i] = e0; K2[i] = e1; }
        else { K1[i] = e1; K2[i] = e0; }
    }

    *k1 = K1; *k2 = K2; nout = nv;
    return RAPHOS_SUCCESS;
}
