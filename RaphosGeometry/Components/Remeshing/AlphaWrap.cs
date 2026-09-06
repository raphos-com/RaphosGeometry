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

namespace Raphos.Geometry.Components.Remeshing
{
    [Guid("118e9adb-17c0-4039-a082-4617f086d71c")]
    public sealed class AlphaWrap : Node
    {
        public AlphaWrap()
            : base(new LocalizableString("Alpha Wrap"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("alpha wrap shrinkwrap watertight envelope offset repair hull");
            Description = new LocalizableString(
                "Produce a watertight shrink-wrap of a messy or open mesh by sampling a signed-distance field "
                + "on a grid and extracting the offset isosurface. Great for sealing scans.");
            GuiPriority = 20; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Input (possibly messy) mesh.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraDouble>("Offset", "Envelope offset distance (0 = auto).", ParameterAccess.Item, new SyneraDouble(0.0));
            InputParameterManager.AddParameter<SyneraInt>("Resolution", "Grid resolution per axis.", ParameterAccess.Item, new SyneraInt(48));

            OutputParameterManager.AddParameter<IMesh>(new LocalizableString("Mesh"), new LocalizableString("Watertight wrapped mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out double offset) |
                !dataAccess.GetData(2, out int resolution))
                return;
            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }

            (Point3D[] points, MeshFace[] faces) = MeshFunctions.AlphaWrap(m.Vertices.ToList(), m.Faces.ToList(), offset, resolution);
            if (points.Length == 0) { AddWarning("The wrap produced an empty mesh; try a larger offset or resolution."); return; }
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(points, faces));
        }
    }
}
