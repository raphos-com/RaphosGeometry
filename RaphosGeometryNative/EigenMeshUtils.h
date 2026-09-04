#pragma once
#include "utils.h"
#include <Eigen/Core>
#include <vector>

// Shared helpers to convert between the interleaved C-buffer interop format
// (xyzxyz… doubles for points, index-triples of Long for faces) and the
// Eigen row-per-element matrices libigl expects. Output buffers are allocated
// with new[] and released by the C# interop via ReleaseMemory*.

namespace RaphosGeo
{
    // Interleaved double* (length nv*3) -> V (nv x 3), Long* (length nf*3) -> F (nf x 3).
    inline void ReadEigenMesh(
        const double* pnts, Long nv,
        const Long* faces, Long nf,
        Eigen::MatrixXd& V, Eigen::MatrixXi& F)
    {
        V.resize(nv, 3);
        for (Long i = 0; i < nv; i++)
        {
            V(i, 0) = pnts[i * 3 + 0];
            V(i, 1) = pnts[i * 3 + 1];
            V(i, 2) = pnts[i * 3 + 2];
        }
        F.resize(nf, 3);
        for (Long i = 0; i < nf; i++)
        {
            F(i, 0) = (int)faces[i * 3 + 0];
            F(i, 1) = (int)faces[i * 3 + 1];
            F(i, 2) = (int)faces[i * 3 + 2];
        }
    }

    // V (n x 3) -> interleaved double* out (allocated new[]), count = n.
    inline void WriteEigenVerts(const Eigen::MatrixXd& V, double** out, Long& count)
    {
        count = (Long)V.rows();
        double* buf = new double[count * 3];
        for (Long i = 0; i < count; i++)
        {
            buf[i * 3 + 0] = V(i, 0);
            buf[i * 3 + 1] = V(i, 1);
            buf[i * 3 + 2] = V(i, 2);
        }
        *out = buf;
    }

    // F (m x 3) -> interleaved Long* out (allocated new[]), count = m.
    inline void WriteEigenFaces(const Eigen::MatrixXi& F, Long** out, Long& count)
    {
        count = (Long)F.rows();
        Long* buf = new Long[count * 3];
        for (Long i = 0; i < count; i++)
        {
            buf[i * 3 + 0] = (Long)F(i, 0);
            buf[i * 3 + 1] = (Long)F(i, 1);
            buf[i * 3 + 2] = (Long)F(i, 2);
        }
        *out = buf;
    }

    // Contiguous scalar column/vector -> double* out (allocated new[]), count = length.
    inline void WriteDoubles(const double* src, Long n, double** out, Long& count)
    {
        count = n;
        double* buf = new double[n];
        for (Long i = 0; i < n; i++) buf[i] = src[i];
        *out = buf;
    }
}
