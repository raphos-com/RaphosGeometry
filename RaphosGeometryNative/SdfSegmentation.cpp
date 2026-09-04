#include "SdfSegmentation.h"
#include "GeoMeshUtils.h"
#include <geogram/basic/common.h>
#include <geogram/mesh/mesh_AABB.h>
#include <geogram/mesh/mesh_geometry.h>
#include <geogram/basic/geometry.h>
#include <vector>
#include <algorithm>
#include <cmath>

// Shape Diameter Function segmentation (Shapira et al. 2008), from scratch. For each facet, cast a
// small cone of rays inward (opposite the facet normal), measure the distance to the opposite
// surface via a Geogram AABB, robustly average -> SDF. Facets are then clustered by log(SDF) with
// 1-D k-means to yield a semantic (thickness-based) segmentation, unlike a dihedral-angle split.
int SdfSegmentation(
    double* pnts, Long nv, Long* faces, Long nf,
    Long nSegments,
    double** sdf, Long& nSdf,
    Long** labels, Long& nLabels
) {
    using namespace GEO;
    initialize();

    Mesh M;
    RaphosGeo::BuildGeoMesh(pnts, nv, faces, nf, M);
    MeshFacetsAABB aabb;
    aabb.initialize(M);

    index_t nfac = M.facets.nb();
    std::vector<double> sdfv(nfac, 0.0);

    // Cone directions (small spread around the inward normal).
    const int NRAYS = 9;
    const double coneDeg = 20.0 * M_PI / 180.0;

    for (index_t f = 0; f < nfac; f++) {
        vec3 c(0, 0, 0);
        for (index_t lv = 0; lv < M.facets.nb_vertices(f); lv++) {
            const double* p = M.vertices.point_ptr(M.facets.vertex(f, lv));
            c += vec3(p[0], p[1], p[2]);
        }
        c /= double(M.facets.nb_vertices(f));
        vec3 n = normalize(GEO::Geom::mesh_facet_normal(M, f));
        vec3 inward = -n;

        // Build a tangent frame for the cone.
        vec3 t = (std::fabs(inward.x) < 0.9) ? vec3(1, 0, 0) : vec3(0, 1, 0);
        vec3 ex = normalize(cross(inward, t));
        vec3 ey = cross(inward, ex);

        vec3 origin = c + inward * 1e-6;   // nudge inside to avoid self-hit
        std::vector<double> hits;
        for (int r = 0; r < NRAYS; r++) {
            double a = coneDeg * (r % 3) / 2.0;
            double ang = (2.0 * M_PI * r) / NRAYS;
            vec3 dir = normalize(inward * std::cos(a) + (ex * std::cos(ang) + ey * std::sin(ang)) * std::sin(a));
            MeshFacetsAABB::Intersection I;
            if (aabb.ray_nearest_intersection(Ray(origin, dir), I)) {
                // weight by alignment with the inward normal
                double d = (I.p - origin).length();
                if (d > 1e-9) hits.push_back(d);
            }
        }
        if (!hits.empty()) {
            std::sort(hits.begin(), hits.end());
            sdfv[f] = hits[hits.size() / 2];   // median
        }
    }

    // 1-D k-means on log(sdf) for k = nSegments.
    int k = (int)(nSegments >= 1 ? nSegments : 2);
    std::vector<double> logv(nfac);
    double lo = 1e30, hi = -1e30;
    for (index_t f = 0; f < nfac; f++) {
        logv[f] = std::log(sdfv[f] + 1e-9);
        lo = std::min(lo, logv[f]); hi = std::max(hi, logv[f]);
    }
    std::vector<double> centers(k);
    for (int i = 0; i < k; i++) centers[i] = lo + (hi - lo) * (i + 0.5) / k;
    std::vector<int> lbl(nfac, 0);
    for (int iter = 0; iter < 20; iter++) {
        for (index_t f = 0; f < nfac; f++) {
            int best = 0; double bd = 1e30;
            for (int i = 0; i < k; i++) { double d = std::fabs(logv[f] - centers[i]); if (d < bd) { bd = d; best = i; } }
            lbl[f] = best;
        }
        std::vector<double> sum(k, 0); std::vector<int> cnt(k, 0);
        for (index_t f = 0; f < nfac; f++) { sum[lbl[f]] += logv[f]; cnt[lbl[f]]++; }
        for (int i = 0; i < k; i++) if (cnt[i]) centers[i] = sum[i] / cnt[i];
    }

    nSdf = (Long)nfac;
    double* sbuf = new double[nfac > 0 ? nfac : 1];
    for (index_t f = 0; f < nfac; f++) sbuf[f] = sdfv[f];
    *sdf = sbuf;

    nLabels = (Long)nfac;
    Long* lbuf = new Long[nfac > 0 ? nfac : 1];
    for (index_t f = 0; f < nfac; f++) lbuf[f] = lbl[f];
    *labels = lbuf;
    return RAPHOS_SUCCESS;
}
