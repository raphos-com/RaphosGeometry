#pragma once
#include "utils.h"

// Configuration for Co3Ne reconstruction. Field order/types must match the C# StructLayout mirror.
struct MeshFromPointCloudConfig {
    double radius = 5;
    Long Psmooth_iter = 0;
    Long nb_neighbors = 30;
    bool repair = true;
    double max_N_angle = 60.0;
    double max_hole_area = 5;
    Long max_hole_edges = 500;
    double min_comp_area = 0.01;
    Long min_comp_facets = 10;
};

/*
    internal static extern RaphosInteropResult MeshFromPointCloud(
        [In] IntPtr pnts, [In] IntPtr normals, [In] long pntLength,
        [In] MeshFromPointCloudConfig cfg,
        [Out] out IntPtr newPnts, [Out] out long newPntsLength,
        [Out] out IntPtr faces,  [Out] out long faceCount);
*/
RAPHOS_EXPORT
int MeshFromPointCloud(
    double* pnts, double* normals, Long pntCount,
    MeshFromPointCloudConfig cfg,
    double** newPnts, Long& newPntsLength,
    Long** faces, Long& faceCount
);
