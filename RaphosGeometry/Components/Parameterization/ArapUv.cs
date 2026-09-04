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

namespace Raphos.Geometry.Components.Parameterization
{
    [Guid("8ae24d3c-f119-402b-b90f-d6a4cfa5a412")]
    public sealed class ArapUv : Node
    {
        public ArapUv()
            : base(new LocalizableString("UV Unwrap (ARAP)"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Parameterization;
            Keywords = new LocalizableString("arap uv unwrap as-rigid-as-possible low distortion parameterization");
            Description = new LocalizableString(
                "As-rigid-as-possible UV unwrapping (free boundary), initialized with a harmonic map and "
                + "refined with ARAP iterations for low angular/area distortion. Requires an open mesh.");
            GuiPriority = 30;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Open triangle mesh to unwrap.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraInt>(
                "Iterations", "Number of ARAP local/global iterations.", ParameterAccess.Item, new SyneraInt(50));

            OutputParameterManager.AddParameter<Point3D>(
                new LocalizableString("UV"),
                new LocalizableString("UV coordinate per vertex as an XY-plane point (mesh vertex order)."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out int iterations))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }
            if (m.IsClosed) { AddError(0, "ARAP unwrapping requires an open mesh with a boundary."); return; }

            (double u, double v)[] uv = MeshFunctions.ArapUv(m.Vertices.ToList(), m.Faces.ToList(), iterations);
            dataAccess.SetListData(0, uv.Select(p => new Point3D(p.u, p.v, 0)).ToArray());
        }
    }
}
