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
    [Guid("99407a2a-9951-4fb0-a6d5-20ad0c7cbd7c")]
    public sealed class WlopConsolidate : Node
    {
        public WlopConsolidate()
            : base(new LocalizableString("WLOP Consolidate"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("wlop consolidate denoise regularize project point cloud even");
            Description = new LocalizableString(
                "Weighted Locally Optimal Projection: denoise and evenly redistribute a point cloud by "
                + "attraction to the input plus inter-point repulsion.");
            GuiPriority = 50; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>("Iterations", "Projection iterations.", ParameterAccess.Item, new SyneraInt(10));
            InputParameterManager.AddParameter<SyneraDouble>("Radius", "Neighbourhood radius (support size).", ParameterAccess.Item, new SyneraDouble(1.0));

            OutputParameterManager.AddParameter<Point3D>(new LocalizableString("Points"), new LocalizableString("Consolidated point cloud."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out int iterations) |
                !dataAccess.GetData(2, out double radius))
                return;
            if (pnts == null || pnts.Count < 3) { AddError(0, "Provide at least three points."); return; }

            dataAccess.SetListData(0, MeshFunctions.WlopConsolidate(pnts, iterations, radius));
        }
    }
}
