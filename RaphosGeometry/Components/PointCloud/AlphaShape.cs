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

namespace Raphos.Geometry.Components.PointCloud
{
    [Guid("b38c5605-0be4-43ae-8e87-6f42de64f05c")]
    public sealed class AlphaShape : Node
    {
        public AlphaShape()
            : base(new LocalizableString("Alpha Shape"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("alpha shape concave hull delaunay boundary reconstruct");
            Description = new LocalizableString(
                "Alpha shape of a point set: the boundary triangles of the Delaunay tetrahedra whose "
                + "circumradius is below alpha. Smaller alpha carves more concavity.");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point set.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraDouble>(
                "Alpha", "Circumradius threshold; larger keeps more (toward the convex hull).",
                ParameterAccess.Item, new SyneraDouble(0.5));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"), new LocalizableString("Alpha-shape surface mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out double alpha))
                return;

            if (pnts == null || pnts.Count < 4)
            {
                AddError(0, "Provide at least four points.");
                return;
            }

            MeshFace[] faces = MeshFunctions.AlphaShape(pnts, alpha);
            if (faces.Length == 0)
            {
                AddWarning("No faces below the alpha threshold; try a larger alpha.");
                return;
            }
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(pnts.ToArray(), faces));
        }
    }
}
