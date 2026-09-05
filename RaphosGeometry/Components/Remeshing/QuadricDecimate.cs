using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Remeshing
{
    [Guid("553e7560-b1f9-42f3-895c-9cedafc8214e")]
    public sealed class QuadricDecimate : Node
    {
        public QuadricDecimate()
            : base(new LocalizableString("Quadric Decimate"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("decimate simplify qem qslim reduce lod");
            Description = new LocalizableString(
                "Reduce a triangle mesh to a target face count using QEM edge-collapse decimation (QSlim).");
            GuiPriority = 10;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Triangle mesh to decimate.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraInt>(
                "Target Faces", "Desired number of triangles in the result.",
                ParameterAccess.Item, new SyneraInt(1000));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"),
                new LocalizableString("Decimated triangle mesh."),
                ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out int target))
                return;

            if (m.QuadCount > 0)
            {
                AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads.");
                return;
            }
            if (target < 1)
            {
                AddError(1, "Target Faces must be at least 1.");
                return;
            }
            if (target >= m.FaceCount)
            {
                AddWarning("Target Faces is not less than the input face count; the mesh is returned unchanged.");
                dataAccess.SetData(0, m);
                return;
            }

            (Point3D[] points, MeshFace[] faces) = MeshFunctions.QuadricDecimate(m.Vertices.ToList(), m.Faces.ToList(), target);
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(points, faces));
        }
    }
}
