using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Localization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.PointCloud
{
    [Guid("d9cbaf71-b3a5-46b8-84eb-a3874f2caaf3")]
    public sealed class BilateralDenoise : Node
    {
        public BilateralDenoise()
            : base(new LocalizableString("Bilateral Denoise"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("bilateral denoise smooth feature preserving point cloud normals");
            Description = new LocalizableString(
                "Feature-preserving point-cloud denoising: move each point along its normal by a bilateral "
                + "(spatial + normal) weighted average of neighbour offsets. Requires per-point normals.");
            GuiPriority = 10; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<Vector3D>("Normals", "Per-point normals.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraDouble>("Sigma Space", "Spatial Gaussian width.", ParameterAccess.Item, new SyneraDouble(0.1));
            InputParameterManager.AddParameter<SyneraDouble>("Sigma Normal", "Offset (feature) Gaussian width.", ParameterAccess.Item, new SyneraDouble(0.1));
            InputParameterManager.AddParameter<SyneraInt>("Iterations", "Denoising passes.", ParameterAccess.Item, new SyneraInt(3));

            OutputParameterManager.AddParameter<Point3D>(new LocalizableString("Points"), new LocalizableString("Denoised point cloud."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetListData(1, out IList<Vector3D> normals) |
                !dataAccess.GetData(2, out double ss) |
                !dataAccess.GetData(3, out double sn) |
                !dataAccess.GetData(4, out int iterations))
                return;
            if (pnts == null || normals == null || pnts.Count != normals.Count || pnts.Count < 2)
            {
                AddError(new int[] { 0, 1 }, "Provide matching non-empty Points and Normals lists.");
                return;
            }

            dataAccess.SetListData(0, MeshFunctions.BilateralDenoise(pnts, normals, ss, sn, iterations));
        }
    }
}
