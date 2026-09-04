using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Localization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Detection
{
    [Guid("9b0cd56b-6ec8-4fc3-98cb-0a4218cff6ab")]
    public sealed class RansacDetect : Node
    {
        public RansacDetect()
            : base(new LocalizableString("RANSAC Shape Detection"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Detection;
            Keywords = new LocalizableString("ransac detect fit plane sphere cylinder primitive segment");
            Description = new LocalizableString(
                "Detect multiple primitives (planes, spheres, cylinders) in a point cloud with efficient RANSAC. "
                + "Outputs a primitive index per point (-1 = unassigned) — extends single-primitive fitting to auto multi-detection.");
            GuiPriority = 10; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraDouble>("Distance", "Inlier distance threshold.", ParameterAccess.Item, new SyneraDouble(0.05));
            InputParameterManager.AddParameter<SyneraInt>("Min Support", "Minimum inliers for a primitive.", ParameterAccess.Item, new SyneraInt(50));
            InputParameterManager.AddParameter<SyneraInt>("Iterations", "Candidate trials per extraction.", ParameterAccess.Item, new SyneraInt(200));

            OutputParameterManager.AddParameter<SyneraInt>(new LocalizableString("Labels"),
                new LocalizableString("Primitive index per point (-1 = unassigned)."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out double dist) |
                !dataAccess.GetData(2, out int minSupport) |
                !dataAccess.GetData(3, out int iterations))
                return;
            if (pnts == null || pnts.Count < 4) { AddError(0, "Provide at least four points."); return; }

            (int[] labels, int[] _) = MeshFunctions.RansacDetect(pnts, dist, minSupport, iterations);
            dataAccess.SetListData(0, labels);
        }
    }
}
