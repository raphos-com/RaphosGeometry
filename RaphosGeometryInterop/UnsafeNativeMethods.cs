using System;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Interop
{
    /// <summary>
    /// P/Invoke declarations into RaphosGeometryNative.dll. Conventions:
    ///   * CallingConvention.Cdecl
    ///   * native int return mapped to <see cref="RaphosInteropResult"/>
    ///   * arrays passed/received as IntPtr, counts as long
    ///   * bool marshalled as UnmanagedType.I1
    /// Each declaration is preceded by a comment quoting the C++ signature.
    /// </summary>
    unsafe struct UnsafeNativeMethods
    {
        const string dll = "RaphosGeometryNative.dll";

        // bool IsAllGood(bool b);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.I1)]
        internal static extern bool IsAllGood(
            [In, MarshalAs(UnmanagedType.I1)] bool b
        );

        // int Sum(double a, double b, double& c);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult Sum(
            [In] double a,
            [In] double b,
            [Out] out double c
        );

        // int ReleaseMemoryLongsOfLongs(Long* ptr);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ReleaseMemoryLongsOfLongs(IntPtr ptr);

        // int ReleaseMemoryDoublesOfDoubles(double* ptr);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern int ReleaseMemoryDoublesOfDoubles(IntPtr ptr);

        // int QuadricDecimate(double* pnts, Long nv, Long* faces, Long nf, Long targetFaces,
        //                     double** oV, Long& onv, Long** oF, Long& onf);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult QuadricDecimate(
            [In] IntPtr pnts, [In] long nv,
            [In] IntPtr faces, [In] long nf,
            [In] long targetFaces,
            [Out] out IntPtr oV, [Out] out long onv,
            [Out] out IntPtr oF, [Out] out long onf
        );

        // int FillHoles(double* pnts, Long nv, Long* faces, Long nf, double maxHoleArea,
        //               Long maxHoleEdges, double** oV, Long& onv, Long** oF, Long& onf,
        //               Long& onPatch, Long** oPatchVCount, Long** oPatchFCount,
        //               double** oPV, Long& oPVtotal, Long** oPF, Long& oPFtotal);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult FillHoles(
            [In] IntPtr pnts, [In] long nv,
            [In] IntPtr faces, [In] long nf,
            [In] double maxHoleArea, [In] long maxHoleEdges,
            [Out] out IntPtr oV, [Out] out long onv,
            [Out] out IntPtr oF, [Out] out long onf,
            [Out] out long onPatch,
            [Out] out IntPtr oPatchVCount, [Out] out IntPtr oPatchFCount,
            [Out] out IntPtr oPV, [Out] out long oPVtotal,
            [Out] out IntPtr oPF, [Out] out long oPFtotal
        );

        // int HeatGeodesicField(double* pnts, Long nv, size_t* faces, Long nf,
        //                       size_t* sources, Long nsources, double** distances, Long& ndist);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult HeatGeodesicField(
            [In] IntPtr pnts, [In] long nv,
            [In] IntPtr faces, [In] long nf,
            [In] IntPtr sources, [In] long nsources,
            [Out] out IntPtr distances, [Out] out long ndist
        );

        // int PrincipalCurvature(double* pnts, Long nv, Long* faces, Long nf, Long radius,
        //                        double** pd1, double** pd2, double** pv1, double** pv2, Long& onv);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult PrincipalCurvature(
            [In] IntPtr pnts, [In] long nv,
            [In] IntPtr faces, [In] long nf,
            [In] long radius,
            [Out] out IntPtr pd1, [Out] out IntPtr pd2,
            [Out] out IntPtr pv1, [Out] out IntPtr pv2,
            [Out] out long onv
        );

        // int WindingNumbers(double* pnts, Long nv, Long* faces, Long nf,
        //                    double* query, Long nq, double** w, Long& onw);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult WindingNumbers(
            [In] IntPtr pnts, [In] long nv,
            [In] IntPtr faces, [In] long nf,
            [In] IntPtr query, [In] long nq,
            [Out] out IntPtr w, [Out] out long onw
        );

        // int MarchingCubes(double* scalars, Long ns, double* gridVerts, Long nx, Long ny, Long nz,
        //                   double isovalue, double** oV, Long& onv, Long** oF, Long& onf);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult MarchingCubes(
            [In] IntPtr scalars, [In] long ns,
            [In] IntPtr gridVerts,
            [In] long nx, [In] long ny, [In] long nz,
            [In] double isovalue,
            [Out] out IntPtr oV, [Out] out long onv,
            [Out] out IntPtr oF, [Out] out long onf
        );

        // int RepairMesh(double* V, Long nv, Long* F, Long nf, double colocateEps, bool triangulate,
        //                double** oV, Long& onv, Long** oF, Long& onf);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult RepairMesh(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] double colocateEpsilon, [In, MarshalAs(UnmanagedType.I1)] bool triangulate,
            [Out] out IntPtr oV, [Out] out long onv, [Out] out IntPtr oF, [Out] out long onf
        );

        // int MakeConsistent(double* V, Long nv, Long* F, Long nf, double** oV, Long& onv, Long** oF, Long& onf);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult MakeConsistent(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [Out] out IntPtr oV, [Out] out long onv, [Out] out IntPtr oF, [Out] out long onf
        );

        // int RemoveSelfIntersections(double* V, Long nv, Long* F, Long nf, Long maxIter,
        //                             double** oV, Long& onv, Long** oF, Long& onf);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult RemoveSelfIntersections(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] long maxIter,
            [Out] out IntPtr oV, [Out] out long onv, [Out] out IntPtr oF, [Out] out long onf
        );

        // int ExactGeodesic(double* V, Long nv, Long* F, Long nf, Long* sources, Long nsources,
        //                   double** distances, Long& ndist);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult ExactGeodesic(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] IntPtr sources, [In] long nsources,
            [Out] out IntPtr distances, [Out] out long ndist
        );

        // int HausdorffDistance(double* VA, Long nva, Long* FA, Long nfa,
        //                       double* VB, Long nvb, Long* FB, Long nfb,
        //                       double& dAB, double& dBA, double& dSym);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult HausdorffDistance(
            [In] IntPtr va, [In] long nva, [In] IntPtr fa, [In] long nfa,
            [In] IntPtr vb, [In] long nvb, [In] IntPtr fb, [In] long nfb,
            [Out] out double dAB, [Out] out double dBA, [Out] out double dSym
        );

        // int LscmUv(double* V, Long nv, Long* F, Long nf, double** uv, Long& nuv);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult LscmUv(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [Out] out IntPtr uv, [Out] out long nuv
        );

        // int MeshFromPointCloud(double* pnts, double* normals, Long pntCount, MeshFromPointCloudConfig cfg,
        //                        double** newPnts, Long& newPntsLength, Long** faces, Long& faceCount);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult MeshFromPointCloud(
            [In] IntPtr pnts, [In] IntPtr normals, [In] long pntLength,
            [In] MeshFromPointCloudConfig cfg,
            [Out] out IntPtr newPnts, [Out] out long newPntsLength,
            [Out] out IntPtr faces, [Out] out long faceCount
        );

        // int CleanPointCloud(double* pnts, Long pntCount, double* newPnts, Long* newPntsCount, Long n, double r);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult CleanPointCloud(
            [In] IntPtr pnts, [In] long pntLength,
            [Out] IntPtr newPnts, [Out] IntPtr newPntsCount,
            [In] long n, [In] double r
        );

        // int HarmonicParam(double* V, Long nv, Long* F, Long nf, double** uv, Long& nuv);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult HarmonicParam(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [Out] out IntPtr uv, [Out] out long nuv
        );

        // int ArapUv(double* V, Long nv, Long* F, Long nf, Long iterations, double** uv, Long& nuv);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult ArapUv(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] long iterations,
            [Out] out IntPtr uv, [Out] out long nuv
        );

        // int EstimateNormals(double* pnts, Long nv, Long k, double** normals, Long& nnormals);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult EstimateNormals(
            [In] IntPtr pnts, [In] long nv, [In] long k,
            [Out] out IntPtr normals, [Out] out long nnormals
        );

        // int FlipoutGeodesic(double* pnts, Long nv, size_t* faces, Long nf, size_t startIdx, size_t endIdx,
        //                     double** newPnts, Long& newPntCount);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult FlipoutGeodesic(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] long startIdx, [In] long endIdx,
            [Out] out IntPtr newPnts, [Out] out long newPntCount
        );

        // int AverageSpacing(double* pnts, Long nv, Long k, double& spacing);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult AverageSpacing(
            [In] IntPtr pnts, [In] long nv, [In] long k, [Out] out double spacing
        );

        // int SimplifyPointCloud(double* pnts, Long nv, double cell, double** outPnts, Long& outCount);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult SimplifyPointCloud(
            [In] IntPtr pnts, [In] long nv, [In] double cell,
            [Out] out IntPtr outPnts, [Out] out long outCount
        );

        // int ManifoldHarmonics(double* V, Long nv, Long* F, Long nf, Long nbEigens,
        //                       double** eigenvalues, Long& nEigenvalues,
        //                       double** eigenvectors, Long& nEigenvectors);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult ManifoldHarmonics(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf, [In] long nbEigens,
            [Out] out IntPtr eigenvalues, [Out] out long nEigenvalues,
            [Out] out IntPtr eigenvectors, [Out] out long nEigenvectors
        );

        // int OrientNormals(double* pnts, Long nv, double* normals, Long k, double** outNormals, Long& nout);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult OrientNormals(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr normals, [In] long k,
            [Out] out IntPtr outNormals, [Out] out long nout
        );

        // int ClipMeshByPlane(double* V, Long nv, Long* F, Long nf, double px,py,pz, double nx,ny,nz,
        //                     double** oV, Long& onv, Long** oF, Long& onf);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult ClipMeshByPlane(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] double px, [In] double py, [In] double pz,
            [In] double nx, [In] double ny, [In] double nz,
            [Out] out IntPtr oV, [Out] out long onv, [Out] out IntPtr oF, [Out] out long onf
        );

        // int ArapDeform(double* V, Long nv, Long* F, Long nf, Long* handles, Long nh, double* targets,
        //                Long iterations, double** oV, Long& onv);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult ArapDeform(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] IntPtr handles, [In] long nh, [In] IntPtr targets,
            [In] long iterations,
            [Out] out IntPtr oV, [Out] out long onv
        );

        // int AlphaShape(double* pnts, Long pntCount, Long** facesPntIndices, Long& facesCount, double alpha);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult AlphaShape(
            [In] IntPtr pnts, [In] long pntCount,
            [Out] out IntPtr facesPntIndices, [Out] out long facesCount,
            [In] double alpha
        );

        // int VectorHeatTransport(double* pnts, Long nv, size_t* faces, Long nf, size_t sourceIdx,
        //                         double sourceX, double sourceY, double** vectors, Long& nvectors);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult VectorHeatTransport(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] long sourceIdx, [In] double svx, [In] double svy, [In] double svz,
            [Out] out IntPtr vectors, [Out] out long nvectors
        );

        // int PoissonReconstruct(double* pnts, double* normals, Long nv, Long depth,
        //                        double** oV, Long& onv, Long** oF, Long& onf);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult PoissonReconstruct(
            [In] IntPtr pnts, [In] IntPtr normals, [In] long nv, [In] long depth,
            [Out] out IntPtr oV, [Out] out long onv, [Out] out IntPtr oF, [Out] out long onf
        );

        // int AutoUvAtlas(double* V, Long nv, Long* F, Long nf, double hardAngle, double** uv, Long& nCornerUv);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult AutoUvAtlas(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] double hardAngle,
            [Out] out IntPtr uv, [Out] out long nCornerUv
        );

        // int BiharmonicWeights(double* V, Long nv, Long* F, Long nf, double* handles, Long nh,
        //                       double** weights, Long& nWeights, Long& nHandlesOut);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult BiharmonicWeights(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] IntPtr handles, [In] long nh,
            [Out] out IntPtr weights, [Out] out long nWeights, [Out] out long nHandlesOut
        );

        // ---- Phase 3-4 from-scratch algorithms ----

        // int RansacDetect(double* pnts, Long nv, double distThreshold, Long minSupport, Long iterations,
        //                  Long** labels, Long& nLabels, Long** types, Long& nTypes);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult RansacDetect(
            [In] IntPtr pnts, [In] long nv, [In] double distThreshold, [In] long minSupport, [In] long iterations,
            [Out] out IntPtr labels, [Out] out long nLabels, [Out] out IntPtr types, [Out] out long nTypes
        );

        // int RegionGrowing(double* pnts, Long nv, double angleDeg, Long k, Long minRegion,
        //                   Long** labels, Long& nLabels, Long& nRegions);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult RegionGrowing(
            [In] IntPtr pnts, [In] long nv, [In] double angleDeg, [In] long k, [In] long minRegion,
            [Out] out IntPtr labels, [Out] out long nLabels, [Out] out long nRegions
        );

        // int JetCurvature(double* pnts, Long nv, Long k, double** k1, double** k2, Long& nout);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult JetCurvature(
            [In] IntPtr pnts, [In] long nv, [In] long k,
            [Out] out IntPtr k1, [Out] out IntPtr k2, [Out] out long nout
        );

        // int WlopConsolidate(double* pnts, Long nv, Long iterations, double radius, double** out, Long& nout);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult WlopConsolidate(
            [In] IntPtr pnts, [In] long nv, [In] long iterations, [In] double radius,
            [Out] out IntPtr outPnts, [Out] out long nout
        );

        // int BilateralDenoise(double* pnts, double* normals, Long nv, double sSpace, double sNormal,
        //                      Long iterations, double** out, Long& nout);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult BilateralDenoise(
            [In] IntPtr pnts, [In] IntPtr normals, [In] long nv,
            [In] double sigmaSpace, [In] double sigmaNormal, [In] long iterations,
            [Out] out IntPtr outPnts, [Out] out long nout
        );

        // int MeanCurvatureSkeleton(double* V, Long nv, Long* F, Long nf, Long iterations, double stepScale,
        //                           double** oV, Long& onv);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult MeanCurvatureSkeleton(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] long iterations, [In] double stepScale,
            [Out] out IntPtr oV, [Out] out long onv
        );

        // int AlphaWrap(double* V, Long nv, Long* F, Long nf, double offset, Long resolution,
        //               double** oV, Long& onv, Long** oF, Long& onf);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult AlphaWrap(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf,
            [In] double offset, [In] long resolution,
            [Out] out IntPtr oV, [Out] out long onv, [Out] out IntPtr oF, [Out] out long onf
        );

        // int SdfSegmentation(double* V, Long nv, Long* F, Long nf, Long nSegments,
        //                     double** sdf, Long& nSdf, Long** labels, Long& nLabels);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult SdfSegmentation(
            [In] IntPtr pnts, [In] long nv, [In] IntPtr faces, [In] long nf, [In] long nSegments,
            [Out] out IntPtr sdf, [Out] out long nSdf, [Out] out IntPtr labels, [Out] out long nLabels
        );

        // int AdvancingFront(double* pnts, Long nv, double radius, Long** faces, Long& faceCount);
        [DllImport(dll, CallingConvention = CallingConvention.Cdecl)]
        internal static extern RaphosInteropResult AdvancingFront(
            [In] IntPtr pnts, [In] long nv, [In] double radius,
            [Out] out IntPtr faces, [Out] out long faceCount
        );
    }
}
