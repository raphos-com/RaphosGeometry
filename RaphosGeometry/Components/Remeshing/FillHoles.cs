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
    [Guid("1bc7a61b-1a91-45ad-a8f5-b94b73509cc0")]
    public sealed class FillHoles : Node
    {
        public FillHoles()
            : base(new LocalizableString("Fill Holes"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Remeshing;
            Keywords = new LocalizableString("hole fill close watertight patch boundary");
            Description = new LocalizableString(
                "Fill boundary holes in a triangle mesh. A maximum hole area of 0 fills every hole.");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Triangle mesh with holes to fill.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraDouble>(
                "Max Hole Area", "Only fill holes up to this area. 0 fills all holes.",
                ParameterAccess.Item, new SyneraDouble(0.0));
            InputParameterManager.AddParameter<SyneraInt>(
                "Max Hole Edges", "Only fill holes whose boundary has up to this many edges. 0 = no limit.",
                ParameterAccess.Item, new SyneraInt(0));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"),
                new LocalizableString("Mesh with holes filled."),
                ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out double maxArea) |
                !dataAccess.GetData(2, out int maxEdges))
                return;

            if (m.QuadCount > 0)
            {
                AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads.");
                return;
            }

            (Point3D[] points, MeshFace[] faces) = MeshFunctions.FillHoles(m.Vertices.ToList(), m.Faces.ToList(), maxArea, maxEdges);
            IMesh result = MeshKernel.CreateFromVerticesAndFaces(points, faces);
            if (!result.IsClosed)
                AddWarning("The result is still not closed; some holes exceeded the area/edge limits.");
            dataAccess.SetData(0, result);
        }
    }
}
