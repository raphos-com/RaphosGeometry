#include "PointCloudDenoise.h"
#include <geogram/basic/common.h>
#include <geogram/points/nn_search.h>
#include <geogram/points/kd_tree.h>
#include <Eigen/Dense>
#include <vector>
#include <cmath>

using V3 = Eigen::Vector3d;

// WLOP: iterated locally-optimal projection with repulsion (Huang 2009).
int WlopConsolidate(
    double* pnts, Long nv, Long iterations, double radius,
    double** out, Long& nout
) {
    using namespace GEO;
    initialize();

    std::vector<V3> Q((size_t)nv), X((size_t)nv);       // Q: fixed input, X: moving projection
    for (Long i = 0; i < nv; i++) { Q[i] = V3(pnts[i*3], pnts[i*3+1], pnts[i*3+2]); X[i] = Q[i]; }

    double h = radius > 0 ? radius : 1.0;
    double invh2 = 16.0 / (h * h);                       // theta(r) = exp(-r^2 * 16/h^2)
    int iters = (int)(iterations > 0 ? iterations : 10);
    index_t K = (index_t)std::min<Long>(24, nv);

    NearestNeighborSearch_var NNq = new BalancedKdTree(3);
    NNq->set_points((index_t)nv, pnts);

    for (int it = 0; it < iters; it++) {
        // kd-tree over the current X positions for the repulsion term.
        std::vector<double> xflat((size_t)nv * 3);
        for (Long i = 0; i < nv; i++) { xflat[i*3]=X[i].x(); xflat[i*3+1]=X[i].y(); xflat[i*3+2]=X[i].z(); }
        NearestNeighborSearch_var NNx = new BalancedKdTree(3);
        NNx->set_points((index_t)nv, xflat.data());

        std::vector<V3> Xn((size_t)nv);
        std::vector<index_t> nb(K); std::vector<double> nd(K);
        for (Long i = 0; i < nv; i++) {
            // Attraction to input Q.
            NNq->get_nearest_neighbors(K, &xflat[i*3], nb.data(), nd.data());
            V3 attract = V3::Zero(); double wsum = 0;
            for (index_t j = 0; j < K; j++) {
                double w = std::exp(-nd[j] * invh2);
                attract += w * Q[nb[j]]; wsum += w;
            }
            attract = wsum > 0 ? V3(attract / wsum) : X[i];
            // Repulsion from neighbouring X.
            NNx->get_nearest_neighbors(K, &xflat[i*3], nb.data(), nd.data());
            V3 repel = V3::Zero(); double rsum = 0;
            for (index_t j = 0; j < K; j++) {
                if (nb[j] == (index_t)i || nd[j] < 1e-16) continue;
                double d = std::sqrt(nd[j]);
                double beta = std::exp(-nd[j] * invh2) / d;
                repel += beta * (X[i] - X[nb[j]]); rsum += beta;
            }
            V3 rep = rsum > 0 ? V3(repel / rsum) : V3(V3::Zero());
            Xn[i] = attract + 0.45 * rep;
        }
        X.swap(Xn);
    }

    nout = nv;
    double* buf = new double[nv * 3];
    for (Long i = 0; i < nv; i++) { buf[i*3]=X[i].x(); buf[i*3+1]=X[i].y(); buf[i*3+2]=X[i].z(); }
    *out = buf;
    return RAPHOS_SUCCESS;
}

// Bilateral denoising along point normals (Fleishman-style, adapted to point sets).
int BilateralDenoise(
    double* pnts, double* normals, Long nv,
    double sigmaSpace, double sigmaNormal, Long iterations,
    double** out, Long& nout
) {
    using namespace GEO;
    initialize();

    std::vector<V3> P((size_t)nv), N((size_t)nv);
    for (Long i = 0; i < nv; i++) {
        P[i] = V3(pnts[i*3], pnts[i*3+1], pnts[i*3+2]);
        N[i] = V3(normals[i*3], normals[i*3+1], normals[i*3+2]).normalized();
    }

    double sd = sigmaSpace > 0 ? sigmaSpace : 1.0;
    double sn = sigmaNormal > 0 ? sigmaNormal : 1.0;
    int iters = (int)(iterations > 0 ? iterations : 3);
    index_t K = (index_t)std::min<Long>(24, nv);

    for (int it = 0; it < iters; it++) {
        std::vector<double> flat((size_t)nv * 3);
        for (Long i = 0; i < nv; i++) { flat[i*3]=P[i].x(); flat[i*3+1]=P[i].y(); flat[i*3+2]=P[i].z(); }
        NearestNeighborSearch_var NN = new BalancedKdTree(3);
        NN->set_points((index_t)nv, flat.data());

        std::vector<V3> Pn = P;
        std::vector<index_t> nb(K); std::vector<double> nd(K);
        for (Long i = 0; i < nv; i++) {
            NN->get_nearest_neighbors(K, &flat[i*3], nb.data(), nd.data());
            double sum = 0, wsum = 0;
            for (index_t j = 0; j < K; j++) {
                if (nb[j] == (index_t)i) continue;
                V3 dp = P[nb[j]] - P[i];
                double t = dp.dot(N[i]);
                double w = std::exp(-nd[j] / (2*sd*sd)) * std::exp(-(t*t) / (2*sn*sn));
                sum += w * t; wsum += w;
            }
            Pn[i] = P[i] + N[i] * (wsum > 0 ? sum / wsum : 0.0);
        }
        P.swap(Pn);
    }

    nout = nv;
    double* buf = new double[nv * 3];
    for (Long i = 0; i < nv; i++) { buf[i*3]=P[i].x(); buf[i*3+1]=P[i].y(); buf[i*3+2]=P[i].z(); }
    *out = buf;
    return RAPHOS_SUCCESS;
}
