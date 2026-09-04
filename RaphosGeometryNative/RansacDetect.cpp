#include "RansacDetect.h"
#include <geogram/basic/common.h>
#include <geogram/points/nn_search.h>
#include <geogram/points/kd_tree.h>
#include <Eigen/Dense>
#include <vector>
#include <random>
#include <cmath>

// Efficient-RANSAC-style multi-primitive detection (Schnabel et al. 2007), from scratch.
// Detects planes, spheres and cylinders. Per-point normals are estimated once by local PCA and
// used for normal-agreement inlier tests. Primitives are extracted greedily: sample a minimal set,
// score inliers, keep the best candidate over a batch of trials, commit it if it has enough
// support, remove its points, and repeat.
namespace {
    using V3 = Eigen::Vector3d;

    struct Prim { int type; V3 a, b; double r; };   // plane: a=point,b=normal | sphere: a=center,r | cyl: a=axisPt,b=axisDir,r

    double planeDist(const Prim& p, const V3& x) { return std::fabs((x - p.a).dot(p.b)); }
    double sphereDist(const Prim& p, const V3& x) { return std::fabs((x - p.a).norm() - p.r); }
    double cylDist(const Prim& p, const V3& x) {
        V3 d = x - p.a; double t = d.dot(p.b); V3 rad = d - t * p.b; return std::fabs(rad.norm() - p.r);
    }
    double primDist(const Prim& p, const V3& x) {
        return p.type == 0 ? planeDist(p, x) : p.type == 1 ? sphereDist(p, x) : cylDist(p, x);
    }

    bool fitSphere(const V3& p0, const V3& p1, const V3& p2, const V3& p3, Prim& out) {
        Eigen::Matrix3d A;
        A.row(0) = (p1 - p0).transpose();
        A.row(1) = (p2 - p0).transpose();
        A.row(2) = (p3 - p0).transpose();
        V3 bvec(0.5 * (p1.squaredNorm() - p0.squaredNorm()),
                0.5 * (p2.squaredNorm() - p0.squaredNorm()),
                0.5 * (p3.squaredNorm() - p0.squaredNorm()));
        if (std::fabs(A.determinant()) < 1e-12) return false;
        V3 c = A.fullPivLu().solve(bvec);
        out.type = 1; out.a = c; out.r = (p0 - c).norm();
        return out.r > 1e-9 && out.r < 1e9;
    }
}

int RansacDetect(
    double* pnts, Long nv,
    double distThreshold, Long minSupport, Long iterations,
    Long** labels, Long& nLabels,
    Long** types, Long& nTypes
) {
    using namespace GEO;
    initialize();

    std::vector<V3> P((size_t)nv), N((size_t)nv);
    for (Long i = 0; i < nv; i++) P[i] = V3(pnts[i * 3 + 0], pnts[i * 3 + 1], pnts[i * 3 + 2]);

    // Per-point normals via local PCA (k neighbours).
    index_t K = (index_t)std::min<Long>(16, nv);
    NearestNeighborSearch_var NN = new BalancedKdTree(3);
    NN->set_points((index_t)nv, pnts);
    {
        std::vector<index_t> nb(K); std::vector<double> nd(K);
        for (Long i = 0; i < nv; i++) {
            NN->get_nearest_neighbors(K, &pnts[i * 3], nb.data(), nd.data());
            V3 c = V3::Zero();
            for (index_t j = 0; j < K; j++) c += P[nb[j]];
            c /= (double)K;
            Eigen::Matrix3d cov = Eigen::Matrix3d::Zero();
            for (index_t j = 0; j < K; j++) { V3 d = P[nb[j]] - c; cov += d * d.transpose(); }
            Eigen::SelfAdjointEigenSolver<Eigen::Matrix3d> es(cov);
            N[i] = es.eigenvectors().col(0).normalized();
        }
    }

    const double cosTol = std::cos(25.0 * M_PI / 180.0);
    Long minSup = minSupport > 3 ? minSupport : 3;
    int trials = (int)(iterations > 0 ? iterations : 200);

    std::vector<Long> label((size_t)nv, -1);
    std::vector<int> primTypes;
    std::mt19937 rng(12345);

    std::vector<Long> remaining((size_t)nv);
    for (Long i = 0; i < nv; i++) remaining[i] = i;

    while ((Long)remaining.size() >= minSup) {
        std::uniform_int_distribution<size_t> pick(0, remaining.size() - 1);

        Prim best; long bestCount = 0; std::vector<char> bestIn;
        for (int t = 0; t < trials; t++) {
            // Candidate primitives from minimal samples.
            std::vector<Prim> cands;
            Long i0 = remaining[pick(rng)], i1 = remaining[pick(rng)], i2 = remaining[pick(rng)], i3 = remaining[pick(rng)];
            if (i0 == i1 || i0 == i2 || i1 == i2) continue;
            // Plane.
            {
                V3 n = (P[i1] - P[i0]).cross(P[i2] - P[i0]);
                if (n.norm() > 1e-12) { Prim p; p.type = 0; p.a = P[i0]; p.b = n.normalized(); cands.push_back(p); }
            }
            // Sphere.
            { Prim s; if (i3 != i0 && i3 != i1 && i3 != i2 && fitSphere(P[i0], P[i1], P[i2], P[i3], s)) cands.push_back(s); }
            // Cylinder from 2 points + their normals.
            {
                V3 axis = N[i0].cross(N[i1]);
                if (axis.norm() > 1e-9) {
                    axis.normalize();
                    // Center: solve nearest point between the two normal lines, projected perpendicular to axis.
                    V3 p0 = P[i0] - axis * (P[i0].dot(axis)), p1 = P[i1] - axis * (P[i1].dot(axis));
                    V3 d0 = (N[i0] - axis * N[i0].dot(axis)), d1 = (N[i1] - axis * N[i1].dot(axis));
                    if (d0.norm() > 1e-9 && d1.norm() > 1e-9) {
                        d0.normalize(); d1.normalize();
                        // Solve p0 + s d0 = p1 + u d1 (least squares in the perp plane).
                        Eigen::Matrix<double, 3, 2> M; M.col(0) = d0; M.col(1) = -d1;
                        V3 rhs = p1 - p0;
                        Eigen::Vector2d su = M.colPivHouseholderQr().solve(rhs);
                        V3 center = p0 + su(0) * d0;
                        double rad = (p0 + su(0) * d0 - (P[i0] - axis * P[i0].dot(axis))).norm();
                        rad = ((P[i0] - axis * P[i0].dot(axis)) - center).norm();
                        if (rad > 1e-6 && rad < 1e6) { Prim c; c.type = 2; c.a = center; c.b = axis; c.r = rad; cands.push_back(c); }
                    }
                }
            }

            for (const Prim& cand : cands) {
                long count = 0;
                std::vector<char> in(remaining.size(), 0);
                for (size_t k = 0; k < remaining.size(); k++) {
                    Long idx = remaining[k];
                    if (primDist(cand, P[idx]) < distThreshold) {
                        // Normal agreement.
                        V3 pn = cand.type == 0 ? cand.b
                              : cand.type == 1 ? (P[idx] - cand.a).normalized()
                              : (P[idx] - cand.a - cand.b * (P[idx] - cand.a).dot(cand.b)).normalized();
                        if (std::fabs(pn.dot(N[idx])) > cosTol) { in[k] = 1; count++; }
                    }
                }
                if (count > bestCount) { bestCount = count; best = cand; bestIn = in; }
            }
        }

        if (bestCount < minSup) break;

        Long primIdx = (Long)primTypes.size();
        primTypes.push_back(best.type);
        std::vector<Long> nextRemaining;
        for (size_t k = 0; k < remaining.size(); k++) {
            if (bestIn[k]) label[remaining[k]] = primIdx;
            else nextRemaining.push_back(remaining[k]);
        }
        remaining.swap(nextRemaining);
    }

    nLabels = nv;
    Long* lbuf = new Long[nv > 0 ? nv : 1];
    for (Long i = 0; i < nv; i++) lbuf[i] = label[i];
    *labels = lbuf;

    nTypes = (Long)primTypes.size();
    Long* tbuf = new Long[nTypes > 0 ? nTypes : 1];
    for (Long i = 0; i < nTypes; i++) tbuf[i] = primTypes[i];
    *types = tbuf;
    return RAPHOS_SUCCESS;
}
