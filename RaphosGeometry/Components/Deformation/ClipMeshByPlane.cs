using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Deformation
{
    [Guid("53dd481e-0ec4-4a95-b0db-3241652c5a06")]
    public sealed class ClipMeshByPlane : Node
    {
        public ClipMeshByPlane()
            : base(new LocalizableString("Clip Mesh by Plane"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("clip cut plane section trim half-space slice mesh");
            Description = new LocalizableString(
                "Clip a triangle mesh with a plane, keeping the half on the back side of the plane normal. "
                + "Straddling triangles are split cleanly along the plane.");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Triangle mesh to clip.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Point3D>("Plane Origin", "A point on the cutting plane.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Vector3D>("Plane Normal", "Plane normal; the half it points away from is kept.", ParameterAccess.Item);

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"), new LocalizableString("Clipped mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out Point3D origin) |
                !dataAccess.GetData(2, out Vector3D normal))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }

            (Point3D[] points, MeshFace[] faces) =
                MeshFunctions.ClipMeshByPlane(m.Vertices.ToList(), m.Faces.ToList(), origin, normal);
            if (points.Length == 0) { AddWarning("The whole mesh was clipped away."); return; }
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(points, faces));
        }
    }
}
