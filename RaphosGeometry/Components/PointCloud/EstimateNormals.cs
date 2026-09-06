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
    [Guid("10298c4c-5df0-49c0-adde-dbdcaf4f2751")]
    public sealed class EstimateNormals : Node
    {
        public EstimateNormals()
            : base(new LocalizableString("Estimate Normals"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("normals estimate pca point cloud neighbours");
            Description = new LocalizableString(
                "Estimate a unit normal at each point by principal-component analysis of its k nearest "
                + "neighbours. Normal orientation (inside/outside) is not resolved here.");
            GuiPriority = 10;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>(
                "Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>(
                "Neighbours", "Number of nearest neighbours used for the PCA fit.", ParameterAccess.Item, new SyneraInt(16));

            OutputParameterManager.AddParameter<Vector3D>(
                new LocalizableString("Normals"),
                new LocalizableString("Estimated unit normal per point (same order as the input)."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out int k))
                return;

            if (pnts == null || pnts.Count < 3)
            {
                AddError(0, "Provide at least three points.");
                return;
            }

            Vector3D[] normals = MeshFunctions.EstimateNormals(pnts, k);
            dataAccess.SetListData(0, normals);
        }
    }
}
