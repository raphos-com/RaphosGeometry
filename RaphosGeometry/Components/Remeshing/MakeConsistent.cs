using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Remeshing
{
    [Guid("2e278888-8498-4c8d-8b4d-3148831c7557")]
    public sealed class MakeConsistent : Node
    {
        public MakeConsistent()
            : base(new LocalizableString("Make Consistent"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Remeshing;
            Keywords = new LocalizableString("orient consistent normals flip winding coherent");
            Description = new LocalizableString(
                "Coherently reorient the facets of a triangle mesh so neighbouring triangles wind the same way.");
            GuiPriority = 40;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Mesh with possibly inconsistent facet orientation.", ParameterAccess.Item);

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"), new LocalizableString("Consistently oriented mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m))
                return;

            (Point3D[] points, MeshFace[] faces) =
                MeshFunctions.MakeConsistent(m.Vertices.ToList(), m.Faces.ToList());
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(points, faces));
        }
    }
}
