using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.PointCloud
{
    [Guid("2971db54-ea5c-4542-ab10-d6bb1d7006c3")]
    public sealed class MeshFromPointCloud : Node
    {
        public MeshFromPointCloud()
            : base(new LocalizableString("Mesh from Point Cloud"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("reconstruct co3ne point cloud mesh surface");
            Description = new LocalizableString(
                "Reconstruct a triangle mesh from a point cloud using Geogram's Co3Ne algorithm. "
                + "Supplying per-point normals improves the result.");
            GuiPriority = 10;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>(
                "Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<Vector3D>(
                "Normals", "Optional per-point normals.", ParameterAccess.List, true);
            InputParameterManager.AddParameter<SyneraDouble>(
                "Radius", "Neighbourhood radius used for reconstruction.", ParameterAccess.Item, new SyneraDouble(1.0));
            InputParameterManager.AddParameter<SyneraInt>(
                "Neighbours", "Number of nearest neighbours per point.", ParameterAccess.Item, new SyneraInt(30));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"), new LocalizableString("Reconstructed triangle mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(2, out double radius) |
                !dataAccess.GetData(3, out int neighbours))
                return;
            dataAccess.GetListData(1, out IList<Vector3D> normals);

            if (pnts == null || pnts.Count < 4)
            {
                AddError(0, "Provide at least four points.");
                return;
            }

            var cfg = new MeshFromPointCloudConfig { radius = radius, nb_neighbors = neighbours };
            (Point3D[] points, MeshFace[] faces) = MeshFunctions.MeshFromPointCloud(pnts, normals, cfg);

            IMesh m = MeshKernel.CreateFromVerticesAndFaces(points, faces);
            if (!m.IsClosed)
                AddWarning("The reconstructed mesh is not closed.");
            dataAccess.SetData(0, m);
        }
    }
}
