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
    [Guid("e90987ac-e0d4-4ef8-9d3a-41c2410adf26")]
    public sealed class RemoveOutliers : Node
    {
        public RemoveOutliers()
            : base(new LocalizableString("Remove Outliers"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("outlier remove clean denoise point cloud radius neighbour");
            Description = new LocalizableString(
                "Remove outlier points whose N-th nearest neighbour lies farther than a given radius "
                + "(Geogram kd-tree).");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>(
                "Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>(
                "Neighbours", "Which nearest neighbour to test (N).", ParameterAccess.Item, new SyneraInt(70));
            InputParameterManager.AddParameter<SyneraDouble>(
                "Radius", "Distance threshold for the N-th neighbour.", ParameterAccess.Item, new SyneraDouble(0.1));

            OutputParameterManager.AddParameter<Point3D>(
                new LocalizableString("Points"), new LocalizableString("Cleaned point cloud."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out int n) |
                !dataAccess.GetData(2, out double r))
                return;

            if (pnts == null || pnts.Count == 0)
            {
                AddError(0, "Provide a non-empty point cloud.");
                return;
            }

            Point3D[] cleaned = MeshFunctions.CleanPointCloud(pnts, n, r);
            dataAccess.SetListData(0, cleaned);
        }
    }
}
