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
    [Guid("a12edaf6-d6fe-41de-b3f7-95377a82c04c")]
    public sealed class PoissonReconstruct : Node
    {
        public PoissonReconstruct()
            : base(new LocalizableString("Poisson Reconstruction"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("poisson reconstruct surface watertight oriented points normals");
            Description = new LocalizableString(
                "Screened Poisson surface reconstruction from an oriented point cloud (requires normals). "
                + "Produces a watertight surface and handles noise better than Co3Ne.");
            GuiPriority = 12;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<Vector3D>("Normals", "Per-point normals (required).", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>(
                "Depth", "Octree depth; higher = more detail (8 default, 10-11 for fine models).",
                ParameterAccess.Item, new SyneraInt(8));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"), new LocalizableString("Reconstructed watertight mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetListData(1, out IList<Vector3D> normals) |
                !dataAccess.GetData(2, out int depth))
                return;

            if (pnts == null || pnts.Count < 4)
            {
                AddError(0, "Provide at least four points.");
                return;
            }
            if (normals == null || normals.Count != pnts.Count)
            {
                AddError(1, "Provide one normal per point (use Estimate + Orient Normals).");
                return;
            }

            (Point3D[] points, MeshFace[] faces) = MeshFunctions.PoissonReconstruct(pnts, normals, depth);
            if (points.Length == 0)
            {
                AddWarning("Reconstruction produced an empty mesh.");
                return;
            }
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(points, faces));
        }
    }
}
