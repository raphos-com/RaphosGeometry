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
    [Guid("86e0fc23-eec8-408a-8a69-53f9abc4683d")]
    public sealed class ExactGeodesic : Node
    {
        public ExactGeodesic()
            : base(new LocalizableString("Exact Geodesic Field"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Analysis;
            Keywords = new LocalizableString("geodesic exact mmp distance field polyhedral");
            Description = new LocalizableString(
                "Exact polyhedral geodesic distance (MMP) from source points to every mesh vertex. "
                + "The exact counterpart to the heat-method field; one value per vertex in mesh order.");
            GuiPriority = 15;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Triangle mesh to compute distances on.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Point3D>(
                "Sources", "Source points; the nearest vertex to each is used as a source.", ParameterAccess.List);

            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Distances"),
                new LocalizableString("Exact geodesic distance at each mesh vertex (mesh vertex order)."),
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
            if (sources == null || sources.Count == 0)
            {
                AddError(1, "Provide at least one source point.");
                return;
            }

            int[] sourceIdx = sources.Select(p => m.VertexTree.QueryNearest(p)).Distinct().ToArray();
            double[] distances = MeshFunctions.ExactGeodesic(m.Vertices.ToList(), m.Faces.ToList(), sourceIdx);
            dataAccess.SetListData(0, distances);
        }
    }
}
