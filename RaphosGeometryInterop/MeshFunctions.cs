using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Interop
{
    /// <summary>
    /// Public marshalling API the Synera nodes call. Ownership discipline (copied from the
    /// proven RaphosTools pattern):
    ///   * inputs are allocated by C# (AllocHGlobal) and freed by C# (FreeHGlobal) in finally;
    ///   * native outputs are pinned while read, then GCHandle.Free() + ReleaseMemory* in finally.
    /// </summary>
    public static class MeshFunctions
    {
        #region output readers

        static Point3D[] ReadPoints(IntPtr ptr, long count)
        {
            var pts = new Point3D[count];
            IntPtr p = ptr;
            int sz = Marshal.SizeOf(typeof(double));
            for (long i = 0; i < count; i++)
            {
                double x = Marshal.PtrToStructure<double>(p); p = IntPtr.Add(p, sz);
                double y = Marshal.PtrToStructure<double>(p); p = IntPtr.Add(p, sz);
                double z = Marshal.PtrToStructure<double>(p); p = IntPtr.Add(p, sz);
                pts[i] = new Point3D(x, y, z);
            }
            return pts;
        }

        static Vector3D[] ReadVectors(IntPtr ptr, long count)
        {
            var vs = new Vector3D[count];
            IntPtr p = ptr;
            int sz = Marshal.SizeOf(typeof(double));
            for (long i = 0; i < count; i++)
            {
                double x = Marshal.PtrToStructure<double>(p); p = IntPtr.Add(p, sz);
                double y = Marshal.PtrToStructure<double>(p); p = IntPtr.Add(p, sz);
                double z = Marshal.PtrToStructure<double>(p); p = IntPtr.Add(p, sz);
                vs[i] = new Vector3D(x, y, z);
            }
            return vs;
        }

        static MeshFace[] ReadFaces(IntPtr ptr, long count)
        {
            var faces = new MeshFace[count];
            IntPtr p = ptr;
            int sz = Marshal.SizeOf(typeof(long));
            for (long i = 0; i < count; i++)
            {
                int a = (int)Marshal.PtrToStructure<long>(p); p = IntPtr.Add(p, sz);
                int b = (int)Marshal.PtrToStructure<long>(p); p = IntPtr.Add(p, sz);
                int c = (int)Marshal.PtrToStructure<long>(p); p = IntPtr.Add(p, sz);
                faces[i] = new MeshFace(a, b, c);
            }
            return faces;
        }

        static double[] ReadDoubles(IntPtr ptr, long count)
        {
            var arr = new double[count];
            if (count > 0) Marshal.Copy(ptr, arr, 0, (int)count);
            return arr;
        }

        static void FreeInput(IntPtr ptr)
        {
            if (ptr != IntPtr.Zero) Marshal.FreeHGlobal(ptr);
        }

        static void ReleaseDoubles(IntPtr ptr, ref GCHandle handle)
        {
            if (ptr != IntPtr.Zero)
            {
                handle.Free();
                UnsafeNativeMethods.ReleaseMemoryDoublesOfDoubles(ptr);
            }
        }

        static void ReleaseLongs(IntPtr ptr, ref GCHandle handle)
        {
            if (ptr != IntPtr.Zero)
            {
                handle.Free();
                UnsafeNativeMethods.ReleaseMemoryLongsOfLongs(ptr);
            }
        }

        #endregion

        /// <summary>QEM edge-collapse decimation to a target triangle count (libigl qslim).</summary>
        public static (Point3D[] points, MeshFace[] faces) QuadricDecimate(IList<Point3D> verts, IList<MeshFace> faces, int targetFaces)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oV = IntPtr.Zero, oF = IntPtr.Zero;
            GCHandle hV = default, hF = default;
            try
            {
                UnsafeNativeMethods.QuadricDecimate(vPtr, verts.Count, fPtr, faces.Count, targetFaces,
                    out oV, out long onv, out oF, out long onf);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return (ReadPoints(oV, onv), ReadFaces(oF, onf));
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oV, ref hV);
                ReleaseLongs(oF, ref hF);
            }
        }

        /// <summary>Fill boundary holes (Geogram). maxHoleArea 0 = fill all.</summary>
        public static (Point3D[] points, MeshFace[] faces) FillHoles(IList<Point3D> verts, IList<MeshFace> faces, double maxHoleArea, int maxHoleEdges)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oV = IntPtr.Zero, oF = IntPtr.Zero;
            GCHandle hV = default, hF = default;
            try
            {
                UnsafeNativeMethods.FillHoles(vPtr, verts.Count, fPtr, faces.Count, maxHoleArea, maxHoleEdges,
                    out oV, out long onv, out oF, out long onf);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return (ReadPoints(oV, onv), ReadFaces(oF, onf));
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oV, ref hV);
                ReleaseLongs(oF, ref hF);
            }
        }

        /// <summary>Heat-method geodesic distance from source vertices to every vertex (Geometry Central).</summary>
        public static double[] HeatGeodesicField(IList<Point3D> verts, IList<MeshFace> faces, IList<int> sourceVertexIndices)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);

            // sources as an interleaved long buffer (size_t is 8 bytes on x64).
            long[] srcs = sourceVertexIndices.Select(i => (long)i).ToArray();
            IntPtr sPtr = Marshal.AllocHGlobal(sizeof(long) * Math.Max(1, srcs.Length));
            Marshal.Copy(srcs, 0, sPtr, srcs.Length);

            IntPtr oD = IntPtr.Zero;
            GCHandle hD = default;
            try
            {
                UnsafeNativeMethods.HeatGeodesicField(vPtr, verts.Count, fPtr, faces.Count,
                    sPtr, srcs.Length, out oD, out long ndist);
                hD = GCHandle.Alloc(oD, GCHandleType.Pinned);
                return ReadDoubles(oD, ndist);
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr); FreeInput(sPtr);
                ReleaseDoubles(oD, ref hD);
            }
        }

        /// <summary>Principal curvature tensor per vertex (libigl). Returns directions and values.</summary>
        public static (Vector3D[] pd1, Vector3D[] pd2, double[] pv1, double[] pv2) PrincipalCurvature(IList<Point3D> verts, IList<MeshFace> faces, int radius)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oPD1 = IntPtr.Zero, oPD2 = IntPtr.Zero, oPV1 = IntPtr.Zero, oPV2 = IntPtr.Zero;
            GCHandle hPD1 = default, hPD2 = default, hPV1 = default, hPV2 = default;
            try
            {
                UnsafeNativeMethods.PrincipalCurvature(vPtr, verts.Count, fPtr, faces.Count, radius,
                    out oPD1, out oPD2, out oPV1, out oPV2, out long onv);
                hPD1 = GCHandle.Alloc(oPD1, GCHandleType.Pinned);
                hPD2 = GCHandle.Alloc(oPD2, GCHandleType.Pinned);
                hPV1 = GCHandle.Alloc(oPV1, GCHandleType.Pinned);
                hPV2 = GCHandle.Alloc(oPV2, GCHandleType.Pinned);
                return (ReadVectors(oPD1, onv), ReadVectors(oPD2, onv), ReadDoubles(oPV1, onv), ReadDoubles(oPV2, onv));
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oPD1, ref hPD1);
                ReleaseDoubles(oPD2, ref hPD2);
                ReleaseDoubles(oPV1, ref hPV1);
                ReleaseDoubles(oPV2, ref hPV2);
            }
        }

        /// <summary>Generalized winding number per query point (libigl). ~1 inside, ~0 outside a closed mesh.</summary>
        public static double[] WindingNumbers(IList<Point3D> verts, IList<MeshFace> faces, IList<Point3D> queries)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            ArrayUtils.ArrayFromPoints(queries, out IntPtr qPtr, out _);
            IntPtr oW = IntPtr.Zero;
            GCHandle hW = default;
            try
            {
                UnsafeNativeMethods.WindingNumbers(vPtr, verts.Count, fPtr, faces.Count,
                    qPtr, queries.Count, out oW, out long onw);
                hW = GCHandle.Alloc(oW, GCHandleType.Pinned);
                return ReadDoubles(oW, onw);
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr); FreeInput(qPtr);
                ReleaseDoubles(oW, ref hW);
            }
        }

        /// <summary>Extract an isosurface mesh from a scalar field on a regular grid (libigl marching cubes).</summary>
        public static (Point3D[] points, MeshFace[] faces) MarchingCubes(
            IList<double> scalars, IList<Point3D> gridVerts, int nx, int ny, int nz, double isovalue)
        {
            double[] s = scalars.ToArray();
            IntPtr sPtr = Marshal.AllocHGlobal(sizeof(double) * Math.Max(1, s.Length));
            Marshal.Copy(s, 0, sPtr, s.Length);
            ArrayUtils.ArrayFromPoints(gridVerts, out IntPtr gvPtr, out _);
            IntPtr oV = IntPtr.Zero, oF = IntPtr.Zero;
            GCHandle hV = default, hF = default;
            try
            {
                UnsafeNativeMethods.MarchingCubes(sPtr, s.Length, gvPtr, nx, ny, nz, isovalue,
                    out oV, out long onv, out oF, out long onf);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return (ReadPoints(oV, onv), ReadFaces(oF, onf));
            }
            finally
            {
                FreeInput(sPtr); FreeInput(gvPtr);
                ReleaseDoubles(oV, ref hV);
                ReleaseLongs(oF, ref hF);
            }
        }

        // Shared mesh-in/mesh-out plumbing for the Geogram repair family.
        delegate RaphosInteropResult MeshInMeshOut(IntPtr v, long nv, IntPtr f, long nf,
            out IntPtr oV, out long onv, out IntPtr oF, out long onf);

        static (Point3D[] points, MeshFace[] faces) RunMeshOp(IList<Point3D> verts, IList<MeshFace> faces, MeshInMeshOut op)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oV = IntPtr.Zero, oF = IntPtr.Zero;
            GCHandle hV = default, hF = default;
            try
            {
                op(vPtr, verts.Count, fPtr, faces.Count, out oV, out long onv, out oF, out long onf);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return (ReadPoints(oV, onv), ReadFaces(oF, onf));
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oV, ref hV);
                ReleaseLongs(oF, ref hF);
            }
        }

        /// <summary>Merge colocated vertices, drop duplicate/degenerate facets, optionally triangulate (Geogram).</summary>
        public static (Point3D[] points, MeshFace[] faces) RepairMesh(IList<Point3D> verts, IList<MeshFace> faces, double colocateEpsilon, bool triangulate)
            => RunMeshOp(verts, faces, (IntPtr v, long nv, IntPtr f, long nf, out IntPtr oV, out long onv, out IntPtr oF, out long onf)
                => UnsafeNativeMethods.RepairMesh(v, nv, f, nf, colocateEpsilon, triangulate, out oV, out onv, out oF, out onf));

        /// <summary>Coherently reorient facets (Geogram).</summary>
        public static (Point3D[] points, MeshFace[] faces) MakeConsistent(IList<Point3D> verts, IList<MeshFace> faces)
            => RunMeshOp(verts, faces, (IntPtr v, long nv, IntPtr f, long nf, out IntPtr oV, out long onv, out IntPtr oF, out long onf)
                => UnsafeNativeMethods.MakeConsistent(v, nv, f, nf, out oV, out onv, out oF, out onf));

        /// <summary>Resolve self-intersections into an intersection-free mesh (Geogram, exact arithmetic).</summary>
        public static (Point3D[] points, MeshFace[] faces) RemoveSelfIntersections(IList<Point3D> verts, IList<MeshFace> faces, int maxIter)
            => RunMeshOp(verts, faces, (IntPtr v, long nv, IntPtr f, long nf, out IntPtr oV, out long onv, out IntPtr oF, out long onf)
                => UnsafeNativeMethods.RemoveSelfIntersections(v, nv, f, nf, maxIter, out oV, out onv, out oF, out onf));

        /// <summary>Exact polyhedral geodesic distance from source vertices to every vertex (libigl MMP).</summary>
        public static double[] ExactGeodesic(IList<Point3D> verts, IList<MeshFace> faces, IList<int> sourceVertexIndices)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            long[] srcs = sourceVertexIndices.Select(i => (long)i).ToArray();
            IntPtr sPtr = Marshal.AllocHGlobal(sizeof(long) * Math.Max(1, srcs.Length));
            Marshal.Copy(srcs, 0, sPtr, srcs.Length);
            IntPtr oD = IntPtr.Zero;
            GCHandle hD = default;
            try
            {
                UnsafeNativeMethods.ExactGeodesic(vPtr, verts.Count, fPtr, faces.Count, sPtr, srcs.Length, out oD, out long ndist);
                hD = GCHandle.Alloc(oD, GCHandleType.Pinned);
                return ReadDoubles(oD, ndist);
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr); FreeInput(sPtr);
                ReleaseDoubles(oD, ref hD);
            }
        }

        /// <summary>Vertex-sampled Hausdorff distance between two meshes: directed A→B, B→A and symmetric.</summary>
        public static (double aToB, double bToA, double symmetric) HausdorffDistance(
            IList<Point3D> vertsA, IList<MeshFace> facesA, IList<Point3D> vertsB, IList<MeshFace> facesB)
        {
            ArrayUtils.ArrayFromPoints(vertsA, out IntPtr vaPtr, out _);
            ArrayUtils.ArrayFromTriFaces(facesA, out IntPtr faPtr, out _);
            ArrayUtils.ArrayFromPoints(vertsB, out IntPtr vbPtr, out _);
            ArrayUtils.ArrayFromTriFaces(facesB, out IntPtr fbPtr, out _);
            try
            {
                UnsafeNativeMethods.HausdorffDistance(
                    vaPtr, vertsA.Count, faPtr, facesA.Count,
                    vbPtr, vertsB.Count, fbPtr, facesB.Count,
                    out double dAB, out double dBA, out double dSym);
                return (dAB, dBA, dSym);
            }
            finally
            {
                FreeInput(vaPtr); FreeInput(faPtr); FreeInput(vbPtr); FreeInput(fbPtr);
            }
        }

        /// <summary>LSCM UV unwrap (libigl). Returns one (u,v) pair per vertex; requires an open surface.</summary>
        public static (double u, double v)[] LscmUv(IList<Point3D> verts, IList<MeshFace> faces)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oUv = IntPtr.Zero;
            GCHandle hUv = default;
            try
            {
                UnsafeNativeMethods.LscmUv(vPtr, verts.Count, fPtr, faces.Count, out oUv, out long nuv);
                hUv = GCHandle.Alloc(oUv, GCHandleType.Pinned);
                double[] flat = ReadDoubles(oUv, nuv * 2);
                var uv = new (double u, double v)[nuv];
                for (long i = 0; i < nuv; i++) uv[i] = (flat[i * 2 + 0], flat[i * 2 + 1]);
                return uv;
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oUv, ref hUv);
            }
        }

        /// <summary>Co3Ne surface reconstruction from a point cloud, optionally with normals (Geogram).</summary>
        public static (Point3D[] points, MeshFace[] faces) MeshFromPointCloud(
            IList<Point3D> pnts, IList<Vector3D> normals, MeshFromPointCloudConfig cfg = default)
        {
            IntPtr normalsPtr = IntPtr.Zero;
            if (normals != null && normals.Count > 0)
                ArrayUtils.ArrayFromPoints(normals, out normalsPtr, out _);
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pntsPtr, out _);
            IntPtr oV = IntPtr.Zero, oF = IntPtr.Zero;
            GCHandle hV = default, hF = default;
            try
            {
                UnsafeNativeMethods.MeshFromPointCloud(pntsPtr, normalsPtr, pnts.Count, cfg,
                    out oV, out long onv, out oF, out long onf);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return (ReadPoints(oV, onv), ReadFaces(oF, onf));
            }
            finally
            {
                FreeInput(pntsPtr);
                if (normalsPtr != IntPtr.Zero) FreeInput(normalsPtr);
                ReleaseDoubles(oV, ref hV);
                ReleaseLongs(oF, ref hF);
            }
        }

        /// <summary>Remove outlier points whose N-th nearest neighbour is farther than radius r (Geogram kd-tree).</summary>
        public static Point3D[] CleanPointCloud(IList<Point3D> pnts, int n = 70, double r = 0.1)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pntsPtr, out _);
            IntPtr countPtr = Marshal.AllocHGlobal(sizeof(long));
            try
            {
                // The native side writes survivors back into pntsPtr (same capacity) and the count.
                UnsafeNativeMethods.CleanPointCloud(pntsPtr, pnts.Count, pntsPtr, countPtr, n, r);
                long kept = Marshal.PtrToStructure<long>(countPtr);
                return ReadPoints(pntsPtr, kept);
            }
            finally
            {
                FreeInput(pntsPtr);
                FreeInput(countPtr);
            }
        }

        // Shared plumbing for UV-parameterization ops (mesh in, (u,v) per vertex out).
        delegate RaphosInteropResult UvOp(IntPtr v, long nv, IntPtr f, long nf, out IntPtr uv, out long nuv);

        static (double u, double v)[] RunUvOp(IList<Point3D> verts, IList<MeshFace> faces, UvOp op)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oUv = IntPtr.Zero;
            GCHandle hUv = default;
            try
            {
                op(vPtr, verts.Count, fPtr, faces.Count, out oUv, out long nuv);
                hUv = GCHandle.Alloc(oUv, GCHandleType.Pinned);
                double[] flat = ReadDoubles(oUv, nuv * 2);
                var uv = new (double u, double v)[nuv];
                for (long i = 0; i < nuv; i++) uv[i] = (flat[i * 2 + 0], flat[i * 2 + 1]);
                return uv;
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oUv, ref hUv);
            }
        }

        /// <summary>Fixed-boundary harmonic UV parameterization (libigl); boundary pinned to a circle.</summary>
        public static (double u, double v)[] HarmonicParam(IList<Point3D> verts, IList<MeshFace> faces)
            => RunUvOp(verts, faces, (IntPtr v, long nv, IntPtr f, long nf, out IntPtr uv, out long nuv)
                => UnsafeNativeMethods.HarmonicParam(v, nv, f, nf, out uv, out nuv));

        /// <summary>As-rigid-as-possible UV parameterization (libigl), harmonic-initialized, free boundary.</summary>
        public static (double u, double v)[] ArapUv(IList<Point3D> verts, IList<MeshFace> faces, int iterations)
            => RunUvOp(verts, faces, (IntPtr v, long nv, IntPtr f, long nf, out IntPtr uv, out long nuv)
                => UnsafeNativeMethods.ArapUv(v, nv, f, nf, iterations, out uv, out nuv));

        /// <summary>Estimate a unit normal per point by PCA over its k nearest neighbours (Geogram kd-tree).</summary>
        public static Vector3D[] EstimateNormals(IList<Point3D> pnts, int k)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            IntPtr oN = IntPtr.Zero;
            GCHandle hN = default;
            try
            {
                UnsafeNativeMethods.EstimateNormals(pPtr, pnts.Count, k, out oN, out long nn);
                hN = GCHandle.Alloc(oN, GCHandleType.Pinned);
                return ReadVectors(oN, nn);
            }
            finally
            {
                FreeInput(pPtr);
                ReleaseDoubles(oN, ref hN);
            }
        }

        /// <summary>FlipOut edge-flip geodesic path between two mesh vertices (Geometry Central).</summary>
        public static Point3D[] FlipoutGeodesic(IList<Point3D> verts, IList<MeshFace> faces, int startIdx, int endIdx)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oP = IntPtr.Zero;
            GCHandle hP = default;
            try
            {
                UnsafeNativeMethods.FlipoutGeodesic(vPtr, verts.Count, fPtr, faces.Count, startIdx, endIdx, out oP, out long np);
                hP = GCHandle.Alloc(oP, GCHandleType.Pinned);
                return ReadPoints(oP, np);
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oP, ref hP);
            }
        }

        /// <summary>Mean nearest-neighbour spacing of a point cloud (Geogram kd-tree).</summary>
        public static double AverageSpacing(IList<Point3D> pnts, int k)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            try
            {
                UnsafeNativeMethods.AverageSpacing(pPtr, pnts.Count, k, out double spacing);
                return spacing;
            }
            finally { FreeInput(pPtr); }
        }

        /// <summary>Voxel-grid downsampling: one centroid per occupied cell of the given size.</summary>
        public static Point3D[] SimplifyPointCloud(IList<Point3D> pnts, double cellSize)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            IntPtr oP = IntPtr.Zero;
            GCHandle hP = default;
            try
            {
                UnsafeNativeMethods.SimplifyPointCloud(pPtr, pnts.Count, cellSize, out oP, out long np);
                hP = GCHandle.Alloc(oP, GCHandleType.Pinned);
                return ReadPoints(oP, np);
            }
            finally
            {
                FreeInput(pPtr);
                ReleaseDoubles(oP, ref hP);
            }
        }

        /// <summary>
        /// Laplace-Beltrami eigenfunctions (Geogram). Returns the eigenvalues and, per eigenfunction,
        /// its value at every vertex (eigenfunctions[j] has one value per vertex, in mesh order).
        /// </summary>
        public static (double[] eigenvalues, double[][] eigenfunctions) ManifoldHarmonics(
            IList<Point3D> verts, IList<MeshFace> faces, int nbEigens)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oVal = IntPtr.Zero, oVec = IntPtr.Zero;
            GCHandle hVal = default, hVec = default;
            try
            {
                UnsafeNativeMethods.ManifoldHarmonics(vPtr, verts.Count, fPtr, faces.Count, nbEigens,
                    out oVal, out long nVal, out oVec, out long nVec);
                hVal = GCHandle.Alloc(oVal, GCHandleType.Pinned);
                hVec = GCHandle.Alloc(oVec, GCHandleType.Pinned);

                double[] vals = ReadDoubles(oVal, nVal);
                double[] flat = ReadDoubles(oVec, nVec);
                int nv = verts.Count;
                int bands = nv > 0 ? (int)(nVec / nv) : 0;
                var funcs = new double[bands][];
                for (int b = 0; b < bands; b++)
                {
                    funcs[b] = new double[nv];
                    System.Array.Copy(flat, (long)b * nv, funcs[b], 0, nv);
                }
                return (vals, funcs);
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oVal, ref hVal);
                ReleaseDoubles(oVec, ref hVec);
            }
        }

        /// <summary>Consistently orient point-cloud normals via MST propagation over the k-NN graph.</summary>
        public static Vector3D[] OrientNormals(IList<Point3D> pnts, IList<Vector3D> normals, int k)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            ArrayUtils.ArrayFromPoints(normals, out IntPtr nPtr, out _);
            IntPtr oN = IntPtr.Zero;
            GCHandle hN = default;
            try
            {
                UnsafeNativeMethods.OrientNormals(pPtr, pnts.Count, nPtr, k, out oN, out long nn);
                hN = GCHandle.Alloc(oN, GCHandleType.Pinned);
                return ReadVectors(oN, nn);
            }
            finally
            {
                FreeInput(pPtr); FreeInput(nPtr);
                ReleaseDoubles(oN, ref hN);
            }
        }

        /// <summary>Clip a mesh by a plane, keeping the half on the negative side of the normal.</summary>
        public static (Point3D[] points, MeshFace[] faces) ClipMeshByPlane(
            IList<Point3D> verts, IList<MeshFace> faces, Point3D planePoint, Vector3D planeNormal)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oV = IntPtr.Zero, oF = IntPtr.Zero;
            GCHandle hV = default, hF = default;
            try
            {
                UnsafeNativeMethods.ClipMeshByPlane(vPtr, verts.Count, fPtr, faces.Count,
                    planePoint.X, planePoint.Y, planePoint.Z,
                    planeNormal.X, planeNormal.Y, planeNormal.Z,
                    out oV, out long onv, out oF, out long onf);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return (ReadPoints(oV, onv), ReadFaces(oF, onf));
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oV, ref hV);
                ReleaseLongs(oF, ref hF);
            }
        }

        /// <summary>As-rigid-as-possible handle-based deformation (libigl). Returns deformed vertices; faces unchanged.</summary>
        public static Point3D[] ArapDeform(
            IList<Point3D> verts, IList<MeshFace> faces, IList<int> handleIndices, IList<Point3D> handleTargets, int iterations)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            long[] h = handleIndices.Select(i => (long)i).ToArray();
            IntPtr hPtr = Marshal.AllocHGlobal(sizeof(long) * Math.Max(1, h.Length));
            Marshal.Copy(h, 0, hPtr, h.Length);
            ArrayUtils.ArrayFromPoints(handleTargets, out IntPtr tPtr, out _);
            IntPtr oV = IntPtr.Zero;
            GCHandle hV = default;
            try
            {
                UnsafeNativeMethods.ArapDeform(vPtr, verts.Count, fPtr, faces.Count,
                    hPtr, h.Length, tPtr, iterations, out oV, out long onv);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                return ReadPoints(oV, onv);
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr); FreeInput(hPtr); FreeInput(tPtr);
                ReleaseDoubles(oV, ref hV);
            }
        }

        /// <summary>Alpha shape of a point set (hand-rolled Delaunay). Returns faces indexing the input points.</summary>
        public static MeshFace[] AlphaShape(IList<Point3D> pnts, double alpha)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            IntPtr oF = IntPtr.Zero;
            GCHandle hF = default;
            try
            {
                UnsafeNativeMethods.AlphaShape(pPtr, pnts.Count, out oF, out long nf, alpha);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return ReadFaces(oF, nf);
            }
            finally
            {
                FreeInput(pPtr);
                ReleaseLongs(oF, ref hF);
            }
        }

        /// <summary>Vector-heat parallel transport of a world-space direction across a mesh (Geometry Central).</summary>
        public static Vector3D[] VectorHeatTransport(IList<Point3D> verts, IList<MeshFace> faces, int sourceIdx, Vector3D sourceDir)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oV = IntPtr.Zero;
            GCHandle hV = default;
            try
            {
                UnsafeNativeMethods.VectorHeatTransport(vPtr, verts.Count, fPtr, faces.Count, sourceIdx, sourceDir.X, sourceDir.Y, sourceDir.Z, out oV, out long nvec);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                return ReadVectors(oV, nvec);
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oV, ref hV);
            }
        }

        /// <summary>Screened Poisson surface reconstruction from oriented points (Geogram / Kazhdan).</summary>
        public static (Point3D[] points, MeshFace[] faces) PoissonReconstruct(
            IList<Point3D> pnts, IList<Vector3D> normals, int depth)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            ArrayUtils.ArrayFromPoints(normals, out IntPtr nPtr, out _);
            IntPtr oV = IntPtr.Zero, oF = IntPtr.Zero;
            GCHandle hV = default, hF = default;
            try
            {
                UnsafeNativeMethods.PoissonReconstruct(pPtr, nPtr, pnts.Count, depth,
                    out oV, out long onv, out oF, out long onf);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return (ReadPoints(oV, onv), ReadFaces(oF, onf));
            }
            finally
            {
                FreeInput(pPtr); FreeInput(nPtr);
                ReleaseDoubles(oV, ref hV);
                ReleaseLongs(oF, ref hF);
            }
        }

        /// <summary>Multi-chart UV atlas (Geogram). Returns one (u,v) per face-corner (3 per triangle, in face order).</summary>
        public static (double u, double v)[] AutoUvAtlas(IList<Point3D> verts, IList<MeshFace> faces, double hardAngle)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oUv = IntPtr.Zero;
            GCHandle hUv = default;
            try
            {
                UnsafeNativeMethods.AutoUvAtlas(vPtr, verts.Count, fPtr, faces.Count, hardAngle, out oUv, out long nc);
                hUv = GCHandle.Alloc(oUv, GCHandleType.Pinned);
                double[] flat = ReadDoubles(oUv, nc * 2);
                var uv = new (double u, double v)[nc];
                for (long i = 0; i < nc; i++) uv[i] = (flat[i * 2 + 0], flat[i * 2 + 1]);
                return uv;
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr);
                ReleaseDoubles(oUv, ref hUv);
            }
        }

        static long[] ReadLongs(IntPtr ptr, long count)
        {
            var arr = new long[count];
            for (long i = 0; i < count; i++)
            {
                arr[i] = Marshal.PtrToStructure<long>(ptr);
                ptr = IntPtr.Add(ptr, sizeof(long));
            }
            return arr;
        }

        /// <summary>RANSAC multi-primitive detection. Returns per-point primitive index (-1 unassigned) and per-primitive type code (0 plane,1 sphere,2 cylinder).</summary>
        public static (int[] labels, int[] primitiveTypes) RansacDetect(IList<Point3D> pnts, double distThreshold, int minSupport, int iterations)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            IntPtr oL = IntPtr.Zero, oT = IntPtr.Zero;
            GCHandle hL = default, hT = default;
            try
            {
                UnsafeNativeMethods.RansacDetect(pPtr, pnts.Count, distThreshold, minSupport, iterations,
                    out oL, out long nL, out oT, out long nT);
                hL = GCHandle.Alloc(oL, GCHandleType.Pinned);
                hT = GCHandle.Alloc(oT, GCHandleType.Pinned);
                int[] labels = ReadLongs(oL, nL).Select(x => (int)x).ToArray();
                int[] types = ReadLongs(oT, nT).Select(x => (int)x).ToArray();
                return (labels, types);
            }
            finally { FreeInput(pPtr); ReleaseLongs(oL, ref hL); ReleaseLongs(oT, ref hT); }
        }

        /// <summary>Region-growing segmentation into smooth regions. Returns per-point region index (-1 if unassigned) and region count.</summary>
        public static (int[] labels, int regionCount) RegionGrowing(IList<Point3D> pnts, double angleDeg, int k, int minRegion)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            IntPtr oL = IntPtr.Zero;
            GCHandle hL = default;
            try
            {
                UnsafeNativeMethods.RegionGrowing(pPtr, pnts.Count, angleDeg, k, minRegion,
                    out oL, out long nL, out long nRegions);
                hL = GCHandle.Alloc(oL, GCHandleType.Pinned);
                int[] labels = ReadLongs(oL, nL).Select(x => (int)x).ToArray();
                return (labels, (int)nRegions);
            }
            finally { FreeInput(pPtr); ReleaseLongs(oL, ref hL); }
        }

        /// <summary>Per-point principal curvatures via jet (Monge quadric) fitting. Returns (k1 max, k2 min).</summary>
        public static (double[] k1, double[] k2) JetCurvature(IList<Point3D> pnts, int k)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            IntPtr oK1 = IntPtr.Zero, oK2 = IntPtr.Zero;
            GCHandle hK1 = default, hK2 = default;
            try
            {
                UnsafeNativeMethods.JetCurvature(pPtr, pnts.Count, k, out oK1, out oK2, out long n);
                hK1 = GCHandle.Alloc(oK1, GCHandleType.Pinned);
                hK2 = GCHandle.Alloc(oK2, GCHandleType.Pinned);
                return (ReadDoubles(oK1, n), ReadDoubles(oK2, n));
            }
            finally { FreeInput(pPtr); ReleaseDoubles(oK1, ref hK1); ReleaseDoubles(oK2, ref hK2); }
        }

        /// <summary>WLOP point-cloud consolidation/denoising (Huang 2009).</summary>
        public static Point3D[] WlopConsolidate(IList<Point3D> pnts, int iterations, double radius)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            IntPtr oP = IntPtr.Zero; GCHandle hP = default;
            try
            {
                UnsafeNativeMethods.WlopConsolidate(pPtr, pnts.Count, iterations, radius, out oP, out long n);
                hP = GCHandle.Alloc(oP, GCHandleType.Pinned);
                return ReadPoints(oP, n);
            }
            finally { FreeInput(pPtr); ReleaseDoubles(oP, ref hP); }
        }

        /// <summary>Bilateral point-cloud denoising along normals.</summary>
        public static Point3D[] BilateralDenoise(IList<Point3D> pnts, IList<Vector3D> normals, double sigmaSpace, double sigmaNormal, int iterations)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            ArrayUtils.ArrayFromPoints(normals, out IntPtr nPtr, out _);
            IntPtr oP = IntPtr.Zero; GCHandle hP = default;
            try
            {
                UnsafeNativeMethods.BilateralDenoise(pPtr, nPtr, pnts.Count, sigmaSpace, sigmaNormal, iterations, out oP, out long n);
                hP = GCHandle.Alloc(oP, GCHandleType.Pinned);
                return ReadPoints(oP, n);
            }
            finally { FreeInput(pPtr); FreeInput(nPtr); ReleaseDoubles(oP, ref hP); }
        }

        /// <summary>Contract a mesh toward its mean-curvature skeleton (Au 2008). Faces unchanged; returns contracted vertices.</summary>
        public static Point3D[] MeanCurvatureSkeleton(IList<Point3D> verts, IList<MeshFace> faces, int iterations, double stepScale)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oV = IntPtr.Zero; GCHandle hV = default;
            try
            {
                UnsafeNativeMethods.MeanCurvatureSkeleton(vPtr, verts.Count, fPtr, faces.Count, iterations, stepScale, out oV, out long onv);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                return ReadPoints(oV, onv);
            }
            finally { FreeInput(vPtr); FreeInput(fPtr); ReleaseDoubles(oV, ref hV); }
        }

        /// <summary>Watertight shrink-wrap of a mesh (signed-distance + marching cubes).</summary>
        public static (Point3D[] points, MeshFace[] faces) AlphaWrap(IList<Point3D> verts, IList<MeshFace> faces, double offset, int resolution)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oV = IntPtr.Zero, oF = IntPtr.Zero;
            GCHandle hV = default, hF = default;
            try
            {
                UnsafeNativeMethods.AlphaWrap(vPtr, verts.Count, fPtr, faces.Count, offset, resolution,
                    out oV, out long onv, out oF, out long onf);
                hV = GCHandle.Alloc(oV, GCHandleType.Pinned);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return (ReadPoints(oV, onv), ReadFaces(oF, onf));
            }
            finally { FreeInput(vPtr); FreeInput(fPtr); ReleaseDoubles(oV, ref hV); ReleaseLongs(oF, ref hF); }
        }

        /// <summary>Shape-diameter-function segmentation. Returns per-face SDF value and per-face segment label.</summary>
        public static (double[] sdf, int[] labels) SdfSegmentation(IList<Point3D> verts, IList<MeshFace> faces, int nSegments)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            IntPtr oS = IntPtr.Zero, oL = IntPtr.Zero;
            GCHandle hS = default, hL = default;
            try
            {
                UnsafeNativeMethods.SdfSegmentation(vPtr, verts.Count, fPtr, faces.Count, nSegments,
                    out oS, out long nS, out oL, out long nL);
                hS = GCHandle.Alloc(oS, GCHandleType.Pinned);
                hL = GCHandle.Alloc(oL, GCHandleType.Pinned);
                return (ReadDoubles(oS, nS), ReadLongs(oL, nL).Select(x => (int)x).ToArray());
            }
            finally { FreeInput(vPtr); FreeInput(fPtr); ReleaseDoubles(oS, ref hS); ReleaseLongs(oL, ref hL); }
        }

        /// <summary>Ball-pivoting (advancing-front) surface reconstruction. Faces index the input points.</summary>
        public static MeshFace[] AdvancingFront(IList<Point3D> pnts, double radius)
        {
            ArrayUtils.ArrayFromPoints(pnts, out IntPtr pPtr, out _);
            IntPtr oF = IntPtr.Zero; GCHandle hF = default;
            try
            {
                UnsafeNativeMethods.AdvancingFront(pPtr, pnts.Count, radius, out oF, out long nf);
                hF = GCHandle.Alloc(oF, GCHandleType.Pinned);
                return ReadFaces(oF, nf);
            }
            finally { FreeInput(pPtr); ReleaseLongs(oF, ref hF); }
        }

        /// <summary>
        /// Bounded biharmonic skinning weights for point handles (libigl). Returns a jagged array
        /// weights[vertex][handle], each vertex's weights summing to 1.
        /// </summary>
        public static double[][] BiharmonicWeights(IList<Point3D> verts, IList<MeshFace> faces, IList<Point3D> handles)
        {
            ArrayUtils.ArrayFromPoints(verts, out IntPtr vPtr, out _);
            ArrayUtils.ArrayFromTriFaces(faces, out IntPtr fPtr, out _);
            ArrayUtils.ArrayFromPoints(handles, out IntPtr hPtr, out _);
            IntPtr oW = IntPtr.Zero;
            GCHandle hW = default;
            try
            {
                UnsafeNativeMethods.BiharmonicWeights(vPtr, verts.Count, fPtr, faces.Count, hPtr, handles.Count,
                    out oW, out long nW, out long nHandles);
                hW = GCHandle.Alloc(oW, GCHandleType.Pinned);
                double[] flat = ReadDoubles(oW, nW);
                int nv = verts.Count;
                int nh = (int)nHandles;
                var w = new double[nv][];
                for (int i = 0; i < nv; i++)
                {
                    w[i] = new double[nh];
                    for (int j = 0; j < nh; j++) w[i][j] = flat[(long)i * nh + j];
                }
                return w;
            }
            finally
            {
                FreeInput(vPtr); FreeInput(fPtr); FreeInput(hPtr);
                ReleaseDoubles(oW, ref hW);
            }
        }
    }
}
