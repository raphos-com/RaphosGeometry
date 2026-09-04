using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Localization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.PointCloud
{
    [Guid("fe80bdef-b836-4730-a270-c7139f659273")]
    public sealed class SimplifyPointCloud : Node
    {
        public SimplifyPointCloud()
            : base(new LocalizableString("Simplify Point Cloud"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("simplify downsample voxel grid decimate point cloud thin");
            Description = new LocalizableString(
                "Downsample a point cloud with a voxel grid: one representative (cell centroid) per occupied "
                + "cell of the given size. Distinct from outlier removal — this thins uniformly.");
            GuiPriority = 25;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraDouble>(
                "Cell Size", "Edge length of the voxel grid cells.", ParameterAccess.Item, new SyneraDouble(1.0));

            OutputParameterManager.AddParameter<Point3D>(
                new LocalizableString("Points"), new LocalizableString("Downsampled point cloud."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out double cell))
                return;

            if (pnts == null || pnts.Count == 0)
            {
                AddError(0, "Provide a non-empty point cloud.");
                return;
            }
            if (cell <= 0)
            {
                AddError(1, "Cell Size must be positive.");
                return;
            }

            Point3D[] simplified = MeshFunctions.SimplifyPointCloud(pnts, cell);
            dataAccess.SetListData(0, simplified);
        }
    }
}
