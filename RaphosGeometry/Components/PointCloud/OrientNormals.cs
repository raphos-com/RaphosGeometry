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
    [Guid("37ea7cb3-29a8-4e3d-882e-4b7498aeb5b6")]
    public sealed class OrientNormals : Node
    {
        public OrientNormals()
            : base(new LocalizableString("Orient Normals"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("orient normals consistent mst propagate flip point cloud");
            Description = new LocalizableString(
                "Consistently orient point-cloud normals by propagating sign along a minimum spanning tree "
                + "of the k-nearest-neighbour graph (Hoppe et al.). Pairs with Estimate Normals.");
            GuiPriority = 10;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<Vector3D>("Normals", "Unoriented per-point normals.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>(
                "Neighbours", "k for the nearest-neighbour graph.", ParameterAccess.Item, new SyneraInt(12));

            OutputParameterManager.AddParameter<Vector3D>(
                new LocalizableString("Normals"), new LocalizableString("Consistently oriented normals."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetListData(1, out IList<Vector3D> normals) |
                !dataAccess.GetData(2, out int k))
                return;

            if (pnts == null || normals == null || pnts.Count != normals.Count || pnts.Count < 2)
            {
                AddError("Provide matching non-empty Points and Normals lists.");
                return;
            }

            Vector3D[] oriented = MeshFunctions.OrientNormals(pnts, normals, k);
            dataAccess.SetListData(0, oriented);
        }
    }
}
