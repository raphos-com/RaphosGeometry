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

namespace Raphos.Geometry.Components.Analysis
{
    [Guid("5696cfd6-14f8-4fe0-87a7-9a9fbd17c4b0")]
    public sealed class HeatGeodesicField : Node
    {
        public HeatGeodesicField()
            : base(new LocalizableString("Heat Geodesic Field"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Analysis;
            Keywords = new LocalizableString("geodesic distance heat method field isolines source");
            Description = new LocalizableString(
                "Geodesic distance from source points to every mesh vertex (heat method). "
                + "Returns a whole-mesh distance field, one value per vertex, aligned to the mesh vertex order.");
            GuiPriority = 10;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Triangle mesh to compute distances on.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Point3D>(
                "Sources", "One or more source points; the nearest vertex to each is used as a source.",
                ParameterAccess.List);

            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Distances"),
                new LocalizableString("Geodesic distance at each mesh vertex (same order as the mesh vertices)."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetListData(1, out IList<Point3D> sources))
                return;

            if (m.QuadCount > 0)
            {
                AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads.");
                return;
            }
            if (!m.IsManifold)
            {
                AddError(0, "Please provide a manifold mesh.");
                return;
            }
            if (sources == null || sources.Count == 0)
            {
                AddError(1, "Provide at least one source point.");
                return;
            }

            int[] sourceIdx = sources.Select(p => m.VertexTree.QueryNearest(p)).Distinct().ToArray();
            double[] distances = MeshFunctions.HeatGeodesicField(m.Vertices.ToList(), m.Faces.ToList(), sourceIdx);
            dataAccess.SetListData(0, distances);
        }
    }
}
