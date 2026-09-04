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
    [Guid("c9c96bbb-63c3-4ebf-bed1-1532bac89e2d")]
    public sealed class CurvatureTensor : Node
    {
        public CurvatureTensor()
            : base(new LocalizableString("Curvature Tensor"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Analysis;
            Keywords = new LocalizableString("curvature principal gaussian mean tensor k1 k2");
            Description = new LocalizableString(
                "Per-vertex principal curvature tensor via robust quadric fitting: principal directions, "
                + "principal curvatures k1/k2, and derived Gaussian (k1*k2) and mean ((k1+k2)/2) curvature.");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>(
                "Mesh", "Triangle mesh to analyse.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraInt>(
                "Radius", "Neighbourhood ring radius used for the quadric fit.",
                ParameterAccess.Item, new SyneraInt(5));

            OutputParameterManager.AddParameter<Vector3D>(
                new LocalizableString("Direction 1"), new LocalizableString("Maximum principal curvature direction."),
                ParameterAccess.List);
            OutputParameterManager.AddParameter<Vector3D>(
                new LocalizableString("Direction 2"), new LocalizableString("Minimum principal curvature direction."),
                ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Curvature 1"), new LocalizableString("Maximum principal curvature k1."),
                ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Curvature 2"), new LocalizableString("Minimum principal curvature k2."),
                ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Gaussian"), new LocalizableString("Gaussian curvature K = k1 * k2."),
                ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Mean"), new LocalizableString("Mean curvature H = (k1 + k2) / 2."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out int radius))
                return;

            if (m.QuadCount > 0)
            {
                AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads.");
                return;
            }

            (Vector3D[] pd1, Vector3D[] pd2, double[] pv1, double[] pv2) =
                MeshFunctions.PrincipalCurvature(m.Vertices.ToList(), m.Faces.ToList(), radius);

            double[] gaussian = pv1.Zip(pv2, (a, b) => a * b).ToArray();
            double[] mean = pv1.Zip(pv2, (a, b) => (a + b) / 2.0).ToArray();

            dataAccess.SetListData(0, pd1);
            dataAccess.SetListData(1, pd2);
            dataAccess.SetListData(2, pv1);
            dataAccess.SetListData(3, pv2);
            dataAccess.SetListData(4, gaussian);
            dataAccess.SetListData(5, mean);
        }
    }
}
