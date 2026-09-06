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
    [Guid("46a4cd5f-53ea-40ae-83fa-a312511dd825")]
    public sealed class AverageSpacing : Node
    {
        public AverageSpacing()
            : base(new LocalizableString("Average Spacing"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("average spacing density local feature size sizing point cloud");
            Description = new LocalizableString(
                "Mean nearest-neighbour spacing of a point cloud: a sizing field useful for choosing radii "
                + "for reconstruction, simplification and remeshing.");
            GuiPriority = 10;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>(
                "Neighbours", "Number of nearest neighbours averaged per point.", ParameterAccess.Item, new SyneraInt(6));

            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Spacing"), new LocalizableString("Mean point spacing."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out int k))
                return;

            if (pnts == null || pnts.Count < 2)
            {
                AddError(0, "Provide at least two points.");
                return;
            }

            double spacing = MeshFunctions.AverageSpacing(pnts, k);
            dataAccess.SetData(0, spacing);
        }
    }
}
