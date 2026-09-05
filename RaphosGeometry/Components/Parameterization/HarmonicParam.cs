using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Parameterization
{
    [Guid("4f9ae427-894a-45ce-96a8-9f22853b7c5f")]
    public sealed class HarmonicParam : Node
    {
        public HarmonicParam()
            : base(new LocalizableString("Harmonic Parameterization"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("harmonic parameterization uv flatten laplacian fixed boundary");
            Description = new LocalizableString(
                "Fixed-boundary harmonic UV parameterization: the mesh boundary is pinned to a circle and "
                + "the interior is solved from the Laplace equation. Requires an open (disk-topology) mesh.");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Open triangle mesh to flatten.", ParameterAccess.Item);

            OutputParameterManager.AddParameter<Point3D>(
                new LocalizableString("UV"),
                new LocalizableString("UV coordinate per vertex as an XY-plane point (mesh vertex order)."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }
            if (m.IsClosed) { AddError(0, "Harmonic parameterization requires an open mesh with a boundary."); return; }

            (double u, double v)[] uv = MeshFunctions.HarmonicParam(m.Vertices.ToList(), m.Faces.ToList());
            dataAccess.SetListData(0, uv.Select(p => new Point3D(p.u, p.v, 0)).ToArray());
        }
    }
}
