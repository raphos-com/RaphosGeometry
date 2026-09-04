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
    [Guid("e91c23e7-6416-4c2e-a93a-d0035dbff43d")]
    public sealed class AutoUvAtlas : Node
    {
        public AutoUvAtlas()
            : base(new LocalizableString("Auto UV Atlas"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Parameterization;
            Keywords = new LocalizableString("uv atlas chart segmentation pack seam texture unwrap");
            Description = new LocalizableString(
                "Segment a mesh into charts along sharp edges and flatten + pack them (Geogram atlas). "
                + "Outputs one UV per face-corner (three per triangle, in face order) so seams are preserved.");
            GuiPriority = 40;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Triangle mesh to atlas.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraDouble>(
                "Hard Angle", "Dihedral angle (degrees) above which a chart boundary is forced.",
                ParameterAccess.Item, new SyneraDouble(45.0));

            OutputParameterManager.AddParameter<Point3D>(
                new LocalizableString("UV"),
                new LocalizableString("UV per face-corner as an XY-plane point (3 per triangle, in face order)."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out double hardAngle))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }

            (double u, double v)[] uv = MeshFunctions.AutoUvAtlas(m.Vertices.ToList(), m.Faces.ToList(), hardAngle);
            if (uv.Length == 0)
            {
                AddWarning("The atlas produced no UVs.");
                return;
            }
            dataAccess.SetListData(0, uv.Select(p => new Point3D(p.u, p.v, 0)).ToArray());
        }
    }
}
