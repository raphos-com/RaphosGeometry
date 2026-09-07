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
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("ransac detect fit plane sphere cylinder primitive segment");
            Description = new LocalizableString(
                "Detect multiple primitives (planes, spheres, cylinders) in a point cloud with efficient RANSAC. "
                + "For each point it reports which primitive it belongs to (Labels: 0,1,2… = the 1st, 2nd, 3rd… "
                + "primitive found, -1 = unassigned) and what kind of shape that is (Shape Type: Plane / Sphere / "
                + "Cylinder / Unassigned). Use Shape Type to colour or filter the cloud by primitive kind, and "
                + "Labels to separate two primitives of the same kind (e.g. two different planes).");
            GuiPriority = 30; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraDouble>("Distance", "Inlier distance threshold.", ParameterAccess.Item, new SyneraDouble(0.05));
            InputParameterManager.AddParameter<SyneraInt>("Min Support", "Minimum inliers for a primitive.", ParameterAccess.Item, new SyneraInt(50));
            InputParameterManager.AddParameter<SyneraInt>("Iterations", "Candidate trials per extraction.", ParameterAccess.Item, new SyneraInt(200));

            OutputParameterManager.AddParameter<SyneraInt>(new LocalizableString("Labels"),
                new LocalizableString("Primitive index per point: 0,1,2… = the 1st, 2nd, 3rd… primitive found (-1 = unassigned). "
                + "Two primitives of the same kind get different labels."), ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraString>(new LocalizableString("Shape Type"),
                new LocalizableString("Primitive kind per point: \"Plane\", \"Sphere\", \"Cylinder\" or \"Unassigned\" — "
                + "the meaning of each label."), ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraInt>(new LocalizableString("Primitive Count"),
                new LocalizableString("Number of primitives detected."), ParameterAccess.Item);
        }

        private static string TypeName(int t) => t == 0 ? "Plane" : t == 1 ? "Sphere" : t == 2 ? "Cylinder" : "Unassigned";

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out double dist) |
                !dataAccess.GetData(2, out int minSupport) |
                !dataAccess.GetData(3, out int iterations))
                return;
            if (pnts == null || pnts.Count < 4) { AddError(0, "Provide at least four points."); return; }

            (int[] labels, int[] types) = MeshFunctions.RansacDetect(pnts, dist, minSupport, iterations);
            var shapeTypes = new string[labels.Length];
            for (int i = 0; i < labels.Length; i++)
                shapeTypes[i] = labels[i] >= 0 && labels[i] < types.Length ? TypeName(types[labels[i]]) : "Unassigned";

            dataAccess.SetListData(0, labels);
            dataAccess.SetListData(1, shapeTypes);
            dataAccess.SetData(2, types.Length);
        }
    }
}
