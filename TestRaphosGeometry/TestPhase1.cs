using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    // These tests exercise the native + interop layers directly on Point3D/MeshFace arrays.
    // They deliberately do NOT boot the Synera application runtime (IMesh/MeshKernel), so they
    // run headlessly under vstest.console; the node classes are covered by build + the Synera
    // node harness separately.
    [TestClass]
    public class TestPhase1
    {
        // A GC after each test surfaces any interop heap corruption from mismanaged unsafe memory.
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        // Number of boundary edges (used by exactly one triangle). 0 => closed surface.
        static int BoundaryEdges(IEnumerable<MeshFace> faces)
        {
            var count = new Dictionary<(int, int), int>();
            void Add(int a, int b)
            {
                var key = a < b ? (a, b) : (b, a);
                count[key] = count.TryGetValue(key, out int c) ? c + 1 : 1;
            }
            foreach (var f in faces) { Add(f.A, f.B); Add(f.B, f.C); Add(f.C, f.A); }
            return count.Values.Count(c => c == 1);
        }

        [TestMethod]
        public void TestNativeSum()
        {
            Assert.AreEqual(5.0, UnsafeUtils.Sum(2.0, 3.0), 1e-12);
            Assert.IsTrue(UnsafeUtils.IsAllGood(false));
        }

        [TestMethod]
        public void TestQuadricDecimate()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 3);   // 1280 faces
            Assert.IsTrue(f.Length > 1000);

            (Point3D[] ov, MeshFace[] of) = MeshFunctions.QuadricDecimate(v, f, 200);
            Assert.IsTrue(ov.Length > 0);
            Assert.IsTrue(of.Length > 0);
            Assert.IsTrue(of.Length < f.Length, $"expected fewer than {f.Length} faces, got {of.Length}");
            Assert.IsTrue(of.Length <= 260, $"expected ~200 faces, got {of.Length}");
        }

        [TestMethod]
        public void TestFillHoles()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 2);
            MeshFace[] holed = f.Take(f.Length - 12).ToArray();
            Assert.IsTrue(BoundaryEdges(holed) > 0, "test setup: mesh should be open");

            (Point3D[] ov, MeshFace[] of, var patches) = MeshFunctions.FillHoles(v, holed, 0.0, 0);
            Assert.IsTrue(of.Length >= holed.Length, "fill holes should not remove faces");
            Assert.AreEqual(0, BoundaryEdges(of), "mesh should be closed after filling all holes");
            Assert.IsTrue(patches.Count >= 1, "at least one hole patch should be reported");
            Assert.IsTrue(patches.All(p => p.faces.Length > 0), "each patch should have faces");
            Assert.AreEqual(of.Length - holed.Length, patches.Sum(p => p.faces.Length),
                "patch faces should account for exactly the facets added by filling");
        }

        [TestMethod]
        public void TestHeatGeodesicField()
        {
            double r = 1.0;
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(r, 3);
            double[] d = MeshFunctions.HeatGeodesicField(v, f, new[] { 0 });

            Assert.AreEqual(v.Length, d.Length);
            Assert.AreEqual(0.0, d[0], 1e-6, "distance at the source vertex must be ~0");
            Assert.IsTrue(d.All(x => x >= -1e-9), "distances must be non-negative");
            Assert.IsTrue(d.Max() <= Math.PI * r * 1.4, $"max distance {d.Max()} too large");
            Assert.IsTrue(d.Max() >= Math.PI * r * 0.6, $"max distance {d.Max()} too small");
        }

        [TestMethod]
        public void TestCurvatureTensor()
        {
            double r = 2.0;                          // true principal curvature = 1/r = 0.5
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(r, 4);
            (Vector3D[] pd1, Vector3D[] pd2, double[] pv1, double[] pv2) =
                MeshFunctions.PrincipalCurvature(v, f, 5);

            Assert.AreEqual(v.Length, pv1.Length);
            double medK1 = pv1.Select(Math.Abs).OrderBy(x => x).ElementAt(pv1.Length / 2);
            double medK2 = pv2.Select(Math.Abs).OrderBy(x => x).ElementAt(pv2.Length / 2);
            Assert.IsTrue(medK1 > 0.35 && medK1 < 0.65, $"median |k1|={medK1}, expected ~0.5");
            Assert.IsTrue(medK2 > 0.35 && medK2 < 0.65, $"median |k2|={medK2}, expected ~0.5");
        }

        [TestMethod]
        public void TestWindingNumber()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 3);
            double[] w = MeshFunctions.WindingNumbers(v, f, new List<Point3D>
            {
                new Point3D(0, 0, 0),      // inside  -> ~1
                new Point3D(5, 5, 5),      // outside -> ~0
            });
            Assert.AreEqual(2, w.Length);
            Assert.IsTrue(Math.Abs(w[0]) > 0.5, $"inside winding {w[0]} should be ~1");
            Assert.IsTrue(Math.Abs(w[1]) < 0.5, $"outside winding {w[1]} should be ~0");
        }

        [TestMethod]
        public void TestMarchingCubes()
        {
            double r = 1.0;
            (List<Point3D> grid, List<double> vals, int n) = MeshMaker.SphereSdfGrid(r, 1.5, 24);
            (Point3D[] v, MeshFace[] f) = MeshFunctions.MarchingCubes(vals, grid, n, n, n, 0.0);

            Assert.IsTrue(v.Length > 0, "isosurface should be non-empty");
            Assert.IsTrue(f.Length > 0);
            double meanR = v.Select(p => Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z)).Average();
            Assert.IsTrue(Math.Abs(meanR - r) < 0.1, $"extracted surface mean radius {meanR}, expected ~{r}");
        }
    }
}
