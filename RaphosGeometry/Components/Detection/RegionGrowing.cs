using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Localization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Detection
{
    [Guid("f51f493a-4084-4e35-8f5a-972afd7bd850")]
    public sealed class RegionGrowing : Node
    {
        public RegionGrowing()
            : base(new LocalizableString("Region Growing"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("region growing segment smooth normal cluster point cloud");
            Description = new LocalizableString(
                "Segment a point cloud into smooth (near-planar) regions by growing from seeds along the "
                + "k-NN graph while normals stay consistent. Outputs a region index per point.");
            GuiPriority = 20; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraDouble>("Angle", "Max normal deviation (degrees) within a region.", ParameterAccess.Item, new SyneraDouble(15.0));
            InputParameterManager.AddParameter<SyneraInt>("Neighbours", "k for the adjacency graph.", ParameterAccess.Item, new SyneraInt(12));
            InputParameterManager.AddParameter<SyneraInt>("Min Region", "Discard regions smaller than this.", ParameterAccess.Item, new SyneraInt(10));

            OutputParameterManager.AddParameter<SyneraInt>(new LocalizableString("Labels"),
                new LocalizableString("Region index per point (-1 = unassigned)."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out double angle) |
                !dataAccess.GetData(2, out int k) |
                !dataAccess.GetData(3, out int minRegion))
                return;
            if (pnts == null || pnts.Count < 3) { AddError(0, "Provide at least three points."); return; }

            (int[] labels, int _) = MeshFunctions.RegionGrowing(pnts, angle, k, minRegion);
            dataAccess.SetListData(0, labels);
        }
    }
}
