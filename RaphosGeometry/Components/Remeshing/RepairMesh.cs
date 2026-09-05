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
    [Guid("7b2fbfa7-8108-42ea-9fd5-e4a14d8156f4")]
    public sealed class RepairMesh : Node
    {
        public RepairMesh()
            : base(new LocalizableString("Repair Mesh"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("repair clean merge coincident duplicate degenerate weld");
            Description = new LocalizableString(
                "Clean up a triangle mesh: merge colocated vertices, remove duplicate and degenerate facets, "
                + "and optionally re-triangulate.");
            GuiPriority = 30;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Mesh to repair.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraDouble>(
                "Merge Tolerance", "Distance below which vertices are merged. 0 merges only exact duplicates.",
                ParameterAccess.Item, new SyneraDouble(0.0));
            InputParameterManager.AddParameter<SyneraBool>(
                "Triangulate", "Re-triangulate the result.",
                ParameterAccess.Item, (IGraphDataType)new SyneraBool(true));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"), new LocalizableString("Repaired mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out double tol) |
                !dataAccess.GetData(2, out bool triangulate))
                return;

            (Point3D[] points, MeshFace[] faces) =
                MeshFunctions.RepairMesh(m.Vertices.ToList(), m.Faces.ToList(), tol, triangulate);
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(points, faces));
        }
    }
}
