#include "QuadricDecimate.h"
#include "EigenMeshUtils.h"
#include <igl/qslim.h>

// QEM edge-collapse decimation (Garland & Heckbert quadric error metric) to a
// target triangle count, via libigl's qslim. libigl is header-only, so qslim.cpp
// is compiled inline here.
int QuadricDecimate(
    double* pnts, Long nv,
    Long* faces, Long nf,
    Long targetFaces,
    double** oV, Long& onv,
    Long** oF, Long& onf
) {
    using namespace RaphosGeo;

    Eigen::MatrixXd V;
    Eigen::MatrixXi F;
    ReadEigenMesh(pnts, nv, faces, nf, V, F);

    Eigen::MatrixXd U;   // decimated vertices
    Eigen::MatrixXi G;   // decimated faces
    Eigen::VectorXi J;   // birth face indices
    Eigen::VectorXi I;   // birth vertex indices

    size_t maxM = (size_t)(targetFaces > 0 ? targetFaces : 1);

    // qslim returns whether it reached the target; U/G are populated either way.
    igl::qslim(V, F, maxM, U, G, J, I);

    WriteEigenVerts(U, oV, onv);
    WriteEigenFaces(G, oF, onf);
    return RAPHOS_SUCCESS;
}
