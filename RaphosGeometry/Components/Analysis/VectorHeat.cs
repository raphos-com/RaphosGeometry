using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Analysis
{
    [Guid("7db18704-93c8-477f-a691-3bd026d6543b")]
    public sealed class VectorHeat : Node
    {
        public VectorHeat()
            : base(new LocalizableString("Vector Heat"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Analysis;
            Keywords = new LocalizableString("vector heat parallel transport tangent field direction");
            Description = new LocalizableString(
                "Parallel-transport a direction from a source point across the whole surface (Vector Heat "
                + "Method). Returns one transported world-space vector per vertex.");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Triangle mesh.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Point3D>("Source", "Source point (mapped to nearest vertex).", ParameterAccess.Item);
            InputParameterManager.AddParameter<Vector3D>("Direction", "World-space direction to transport from the source.", ParameterAccess.Item);

            OutputParameterManager.AddParameter<Vector3D>(
                new LocalizableString("Vectors"),
                new LocalizableString("Transported vector at each vertex (mesh vertex order)."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out Point3D source) |
                !dataAccess.GetData(2, out Vector3D direction))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }
            if (!m.IsManifold) { AddError(0, "Please provide a manifold mesh."); return; }

            int sourceIdx = m.VertexTree.QueryNearest(source);
            Vector3D[] vectors = MeshFunctions.VectorHeatTransport(m.Vertices.ToList(), m.Faces.ToList(), sourceIdx, direction);
            dataAccess.SetListData(0, vectors);
        }
    }
}
