using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Geometry;
using Synera.Localization;
using System;
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
            Keywords = new LocalizableString("ransac detect fit plane sphere cylinder primitive segment shape");
            Description = new LocalizableString(
                "Detect multiple primitives (planes, spheres, cylinders) in a point cloud with efficient RANSAC. "
                + "For each point it reports which primitive it belongs to (Labels: 0,1,2… = the 1st, 2nd, 3rd… "
                + "primitive found, -1 = unassigned) and what kind of shape that is (Shape Type). It also returns "
                + "the actual fitted geometry as solids (Fitted Shapes) — the plane patches, spheres and cylinders "
                + "recovered from the scan, each trimmed to the extent of its own points — ready for measurement, "
                + "booleans or CAD.");
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
            OutputParameterManager.AddParameter<IBody>(new LocalizableString("Fitted Shapes"),
                new LocalizableString("The actual fitted primitive per detected shape (one solid per label, in label order): "
                + "a trimmed plane patch, a sphere or a cylinder, each bounded to the extent of its inlier points."),
                ParameterAccess.List);
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

            (int[] labels, int[] types, Point3D[] a, Vector3D[] b, double[] radius) =
                MeshFunctions.RansacDetect(pnts, dist, minSupport, iterations);
            int nPrim = types.Length;

            // per-point shape-type name (the mapping from each label to its kind)
            var shapeTypes = new string[labels.Length];
            for (int i = 0; i < labels.Length; i++)
                shapeTypes[i] = labels[i] >= 0 && labels[i] < nPrim ? TypeName(types[labels[i]]) : "Unassigned";

            // gather inlier points per primitive so each fitted shape can be trimmed to its own extent
            var inliers = new List<Point3D>[nPrim];
            for (int p = 0; p < nPrim; p++) inliers[p] = new List<Point3D>();
            for (int i = 0; i < labels.Length; i++)
                if (labels[i] >= 0 && labels[i] < nPrim) inliers[labels[i]].Add(pnts[i]);

            var shapes = new List<IBody>(nPrim);
            for (int p = 0; p < nPrim; p++)
            {
                IBody body = BuildPrimitive(types[p], a[p], b[p], radius[p], inliers[p]);
                if (body != null) shapes.Add(body);
            }

            dataAccess.SetListData(0, labels);
            dataAccess.SetListData(1, shapeTypes);
            dataAccess.SetListData(2, shapes);
            dataAccess.SetData(3, nPrim);
        }

        /// <summary>Build the fitted primitive as a solid, trimmed to the extent of its inlier points.</summary>
        private static IBody BuildPrimitive(int type, Point3D a, Vector3D b, double r, List<Point3D> inliers)
        {
            if (inliers.Count == 0) return null;

            if (type == 1) // sphere
                return GeometryKernel.CreateSphere(r, a);

            if (type == 0) // plane -> a thin box patch spanning the inliers in the plane
            {
                var pl = new Plane(a, b);
                double uMin = double.PositiveInfinity, uMax = double.NegativeInfinity;
                double vMin = double.PositiveInfinity, vMax = double.NegativeInfinity;
                foreach (var q in inliers)
                {
                    pl.LocalCoordinatesAt(q, out double u, out double v, out double _w);
                    uMin = Math.Min(uMin, u); uMax = Math.Max(uMax, u);
                    vMin = Math.Min(vMin, v); vMax = Math.Max(vMax, v);
                }
                double thick = 0.02 * Math.Max(uMax - uMin, vMax - vMin);
                if (!(thick > 0)) thick = 1e-4;
                return GeometryKernel.CreateBoxBody(pl,
                    new Interval1D(uMin, uMax), new Interval1D(vMin, vMax), new Interval1D(-thick, thick));
            }

            // cylinder -> trim along the axis to the inlier extent
            var axisPlane = new Plane(a, b);
            double tMin = double.PositiveInfinity, tMax = double.NegativeInfinity;
            foreach (var q in inliers)
            {
                axisPlane.LocalCoordinatesAt(q, out double _u, out double _v, out double w);
                tMin = Math.Min(tMin, w); tMax = Math.Max(tMax, w);
            }
            double height = tMax - tMin;
            if (!(height > 0)) return null;
            Point3D basePt = a + b.Normalized() * tMin;
            return GeometryKernel.CreateCylinder(r, height, new Plane(basePt, b));
        }
    }
}
