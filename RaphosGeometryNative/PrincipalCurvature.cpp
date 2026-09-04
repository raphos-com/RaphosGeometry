#include "PrincipalCurvature.h"
#include "EigenMeshUtils.h"
#include <igl/principal_curvature.h>

// Robust quadric-fit principal curvature tensor (Panozzo et al.): per-vertex
// principal directions PD1/PD2 (max/min) and principal values PV1/PV2. Gaussian
// (K = PV1*PV2) and mean (H = (PV1+PV2)/2) curvatures are derived on the C# side.
int PrincipalCurvature(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long radius,
    double** pd1, double** pd2,
    double** pv1, double** pv2,
    Long& onv
) {
    using namespace RaphosGeo;

    Eigen::MatrixXd V;
    Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::MatrixXd PD1, PD2;
    Eigen::VectorXd PV1, PV2;
    unsigned r = (unsigned)(radius > 0 ? radius : 5);
    igl::principal_curvature(V, F, PD1, PD2, PV1, PV2, r, true);

    WriteEigenVerts(PD1, pd1, onv);
    WriteEigenVerts(PD2, pd2, onv);
    WriteDoubles(PV1.data(), (Long)PV1.size(), pv1, onv);
    WriteDoubles(PV2.data(), (Long)PV2.size(), pv2, onv);
    return RAPHOS_SUCCESS;
}
