using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2g
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        [TestMethod]
        public void TestClipMeshByPlane()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 3);
            // Keep the lower half (z <= 0): plane through origin, normal +Z.
            (Point3D[] ov, MeshFace[] of) = MeshFunctions.ClipMeshByPlane(
                v.ToList(), f.ToList(), new Point3D(0, 0, 0), new Vector3D(0, 0, 1));

            Assert.IsTrue(ov.Length > 0 && of.Length > 0, "clipped mesh should be non-empty");
            Assert.IsTrue(ov.All(p => p.Z <= 1e-6), "all kept vertices must be on/under the plane");
            Assert.IsTrue(ov.Any(p => p.Z < -0.5), "the lower cap should be retained");
            // The cut should have created vertices right on the plane (the boundary circle).
            Assert.IsTrue(ov.Count(p => Math.Abs(p.Z) < 1e-6) >= 3, "expected a cut boundary on the plane");
        }

        [TestMethod]
        public void TestArapDeform()
        {
            // A flat grid; pin the four corners, then lift the centre handle by +Z.
            (Point3D[] v, MeshFace[] f) = MeshMaker.Grid(9);  // 81 verts, unit square
            int n = 9;
            int corner0 = 0, corner1 = n - 1, corner2 = n * (n - 1), corner3 = n * n - 1;
            int centre = (n / 2) * n + (n / 2);

            var handles = new List<int> { corner0, corner1, corner2, corner3, centre };
            var targets = new List<Point3D>
            {
                v[corner0], v[corner1], v[corner2], v[corner3],
                new Point3D(v[centre].X, v[centre].Y, 0.5),   // lift the centre
            };

            Point3D[] deformed = MeshFunctions.ArapDeform(v.ToList(), f.ToList(), handles, targets, 100);
            Assert.AreEqual(v.Length, deformed.Length);
            // The centre should have moved up toward the target; corners should stay put.
            Assert.IsTrue(deformed[centre].Z > 0.3, $"centre should be lifted, z={deformed[centre].Z}");
            Assert.IsTrue(Math.Abs(deformed[corner0].Z) < 1e-3, "pinned corner should not move in Z");
        }
    }
}
