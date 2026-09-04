#pragma once
#include "utils.h"

// Average distance from each point to its k nearest neighbours, averaged over the cloud.
RAPHOS_EXPORT
int AverageSpacing(double* pnts, Long nv, Long k, double& spacing);

// Voxel-grid downsampling: keep one representative (cell centroid) per occupied cell of size cell.
RAPHOS_EXPORT
int SimplifyPointCloud(
    double* pnts, Long nv, double cell,
    double** outPnts, Long& outCount
);
