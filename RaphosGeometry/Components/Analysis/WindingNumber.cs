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
    [Guid("938c070a-98ec-441c-a16b-d3c10bcf27a4")]
    public sealed class WindingNumber : Node
    {
        public WindingNumber()
            : base(new LocalizableString("Winding Number"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Analysis;
            Keywords = new LocalizableString("winding number inside outside containment point in mesh");
            Description = new LocalizableString(
                "Generalized winding number of each query point with respect to a mesh: approximately 1 inside "
                + "and 0 outside a closed mesh, and robust on imperfect or open meshes.");
            GuiPriority = 30;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Reference triangle mesh.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Point3D>(
                "Points", "Query points to test.", ParameterAccess.List);

            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Winding"),
                new LocalizableString("Winding number per query point (~1 inside, ~0 outside)."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetListData(1, out IList<Point3D> points))
                return;

            if (m.QuadCount > 0)
            {
                AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads.");
                return;
            }
            if (points == null || points.Count == 0)
            {
                AddError(1, "Provide at least one query point.");
                return;
            }

            double[] winding = MeshFunctions.WindingNumbers(m.Vertices.ToList(), m.Faces.ToList(), points);
            dataAccess.SetListData(0, winding);
        }
    }
}
