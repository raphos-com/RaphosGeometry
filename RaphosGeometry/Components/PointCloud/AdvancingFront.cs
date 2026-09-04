using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.PointCloud
{
    [Guid("378485b2-ba5a-4ce0-b422-5ef526bb2d90")]
    public sealed class AdvancingFront : Node
    {
        public AdvancingFront()
            : base(new LocalizableString("Advancing Front"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("advancing front ball pivoting reconstruct surface points mesh");
            Description = new LocalizableString(
                "Reconstruct a triangle mesh from points by ball-pivoting (an advancing-front method): a ball "
                + "of the given radius rolls over the samples emitting triangles. Radius 0 auto-picks from spacing.");
            GuiPriority = 14; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraDouble>("Radius", "Pivoting ball radius (0 = auto).", ParameterAccess.Item, new SyneraDouble(0.0));

            OutputParameterManager.AddParameter<IMesh>(new LocalizableString("Mesh"), new LocalizableString("Reconstructed mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out double radius))
                return;
            if (pnts == null || pnts.Count < 4) { AddError(0, "Provide at least four points."); return; }

            MeshFace[] faces = MeshFunctions.AdvancingFront(pnts, radius);
            if (faces.Length == 0) { AddWarning("No triangles were produced; try a larger radius."); return; }
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(pnts.ToArray(), faces));
        }
    }
}
