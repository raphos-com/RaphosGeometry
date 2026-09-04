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

namespace Raphos.Geometry.Components.Detection
{
    [Guid("0900e5c4-258a-4acd-b1aa-9f12a4944746")]
    public sealed class SdfSegmentation : Node
    {
        public SdfSegmentation()
            : base(new LocalizableString("SDF Segmentation"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Detection;
            Keywords = new LocalizableString("sdf shape diameter segment part thickness semantic cluster");
            Description = new LocalizableString(
                "Segment a mesh by the Shape Diameter Function (local thickness), giving a semantic part split "
                + "rather than a dihedral-angle split. Outputs per-face SDF value and per-face segment label.");
            GuiPriority = 40; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Triangle mesh to segment.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraInt>("Segments", "Number of segments (clusters).", ParameterAccess.Item, new SyneraInt(2));

            OutputParameterManager.AddParameter<SyneraDouble>(new LocalizableString("SDF"), new LocalizableString("Shape diameter per face."), ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraInt>(new LocalizableString("Segments"), new LocalizableString("Segment index per face."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out int segments))
                return;
            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }

            (double[] sdf, int[] labels) = MeshFunctions.SdfSegmentation(m.Vertices.ToList(), m.Faces.ToList(), segments);
            dataAccess.SetListData(0, sdf);
            dataAccess.SetListData(1, labels);
        }
    }
}
