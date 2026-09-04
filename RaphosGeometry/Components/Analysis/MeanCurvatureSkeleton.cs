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

namespace Raphos.Geometry.Components.Analysis
{
    [Guid("f69a6477-9554-4d77-aa03-8e3b38e749ff")]
    public sealed class MeanCurvatureSkeleton : Node
    {
        public MeanCurvatureSkeleton()
            : base(new LocalizableString("Mean-Curvature Skeleton"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Analysis;
            Keywords = new LocalizableString("skeleton medial contraction laplacian mean curvature centreline");
            Description = new LocalizableString(
                "Contract a mesh toward its curve skeleton via implicit mean-curvature (Laplacian) flow. "
                + "Tubular parts collapse onto their centrelines; connectivity is preserved.");
            GuiPriority = 70; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Triangle mesh to contract.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraInt>("Iterations", "Contraction iterations.", ParameterAccess.Item, new SyneraInt(5));
            InputParameterManager.AddParameter<SyneraDouble>("Step", "Contraction step scale.", ParameterAccess.Item, new SyneraDouble(0.1));

            OutputParameterManager.AddParameter<IMesh>(new LocalizableString("Mesh"), new LocalizableString("Contracted (skeletal) mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out int iterations) |
                !dataAccess.GetData(2, out double step))
                return;
            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }

            Point3D[] contracted = MeshFunctions.MeanCurvatureSkeleton(m.Vertices.ToList(), m.Faces.ToList(), iterations, step);
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(contracted, m.Faces.ToArray()));
        }
    }
}
