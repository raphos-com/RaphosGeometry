using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Geometry;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Analysis
{
    [Guid("66658137-8df3-4def-8ec9-9df08188196d")]
    public sealed class FlipoutGeodesicPath : Node
    {
        public FlipoutGeodesicPath()
            : base(new LocalizableString("Geodesic Path"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Analysis;
            Keywords = new LocalizableString("geodesic path flipout shortest curve on mesh");
            Description = new LocalizableString(
                "Shortest geodesic path along a mesh surface between two points, via the FlipOut edge-flip "
                + "algorithm. The nearest vertex to each input point is used as an endpoint.");
            GuiPriority = 50;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Triangle mesh to walk along.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Point3D>("Start", "Start point.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Point3D>("End", "End point.", ParameterAccess.Item);

            OutputParameterManager.AddParameter<IPolyline>(
                new LocalizableString("Geodesic"), new LocalizableString("Geodesic path polyline."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out Point3D s) |
                !dataAccess.GetData(2, out Point3D e))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }
            if (!m.IsManifold) { AddError(0, "Please provide a manifold mesh."); return; }

            int sIdx = m.VertexTree.QueryNearest(s);
            int eIdx = m.VertexTree.QueryNearest(e);
            if (sIdx == eIdx)
            {
                AddError(new int[] { 1, 2 }, "The start and end points map to the same vertex.");
                return;
            }

            Point3D[] pnts = MeshFunctions.FlipoutGeodesic(m.Vertices.ToList(), m.Faces.ToList(), sIdx, eIdx);
            dataAccess.SetData(0, GeometryKernel.CreatePolyline(pnts));
        }
    }
}
