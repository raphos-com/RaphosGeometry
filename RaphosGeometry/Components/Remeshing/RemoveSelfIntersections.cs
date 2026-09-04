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
    [Guid("6c52b68a-3973-462c-be37-7da0ae3d8620")]
    public sealed class RemoveSelfIntersections : Node
    {
        public RemoveSelfIntersections()
            : base(new LocalizableString("Remove Self-Intersections"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Remeshing;
            Keywords = new LocalizableString("self intersection resolve clean overlap exact");
            Description = new LocalizableString(
                "Resolve self-intersections in a triangle mesh into a clean, intersection-free triangulation "
                + "using exact arithmetic.");
            GuiPriority = 50;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Self-intersecting triangle mesh.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraInt>(
                "Max Iterations", "Maximum number of resolution passes.",
                ParameterAccess.Item, new SyneraInt(3));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"), new LocalizableString("Intersection-free mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out int maxIter))
                return;

            (Point3D[] points, MeshFace[] faces) =
                MeshFunctions.RemoveSelfIntersections(m.Vertices.ToList(), m.Faces.ToList(), maxIter);
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(points, faces));
        }
    }
}
