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
    [Guid("6fa9d32f-731e-477a-846f-def0a3e4d26b")]
    public sealed class UvUnwrapLscm : Node
    {
        public UvUnwrapLscm()
            : base(new LocalizableString("UV Unwrap (LSCM)"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Parameterization;
            Keywords = new LocalizableString("uv unwrap lscm conformal parameterization flatten texture");
            Description = new LocalizableString(
                "Least-squares conformal UV unwrapping of an open (disk-topology) mesh. "
                + "Outputs one UV coordinate per vertex as points in the XY plane (Z = 0).");
            GuiPriority = 10;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Open triangle mesh to unwrap (must have a boundary).", ParameterAccess.Item);

            OutputParameterManager.AddParameter<Point3D>(
                new LocalizableString("UV"),
                new LocalizableString("UV coordinate per vertex as an XY-plane point (mesh vertex order)."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m))
                return;

            if (m.QuadCount > 0)
            {
                AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads.");
                return;
            }
            if (m.IsClosed)
            {
                AddError(0, "LSCM requires an open mesh with a boundary; this mesh is closed.");
                return;
            }

            (double u, double v)[] uv = MeshFunctions.LscmUv(m.Vertices.ToList(), m.Faces.ToList());
            Point3D[] uvPoints = uv.Select(p => new Point3D(p.u, p.v, 0)).ToArray();
            dataAccess.SetListData(0, uvPoints);
        }
    }
}
