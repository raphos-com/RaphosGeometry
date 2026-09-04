using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2b
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        [TestMethod]
        public void TestExactGeodesic()
        {
            double r = 1.0;
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(r, 3);
            double[] d = MeshFunctions.ExactGeodesic(v.ToList(), f.ToList(), new[] { 0 });

            Assert.AreEqual(v.Length, d.Length);
            Assert.AreEqual(0.0, d[0], 1e-9, "distance at source must be 0");
            Assert.IsTrue(d.All(x => x >= -1e-9));
            // Exact geodesic max on a sphere is the antipode distance pi*r; the polyhedral
            // approximation slightly under-estimates the smooth value.
            Assert.IsTrue(d.Max() <= Math.PI * r * 1.05, $"max distance {d.Max()} too large");
            Assert.IsTrue(d.Max() >= Math.PI * r * 0.85, $"max distance {d.Max()} too small");
        }

        [TestMethod]
        public void TestHausdorffDistance()
        {
            (Point3D[] va, MeshFace[] fa) = MeshMaker.Icosphere(1.0, 3);
            (Point3D[] vb, MeshFace[] fb) = MeshMaker.Icosphere(1.0, 3);

            // Identical meshes -> ~0.
            (double aa, double bb, double sym0) = MeshFunctions.HausdorffDistance(va.ToList(), fa.ToList(), vb.ToList(), fb.ToList());
            Assert.IsTrue(sym0 < 1e-6, $"identical meshes should have ~0 Hausdorff, got {sym0}");

            // Sphere r=1 vs r=1.2: vertex-sampled deviation ~0.2.
            (Point3D[] vc, MeshFace[] fc) = MeshMaker.Icosphere(1.2, 3);
            (double _, double __, double sym) = MeshFunctions.HausdorffDistance(va.ToList(), fa.ToList(), vc.ToList(), fc.ToList());
            Assert.IsTrue(sym > 0.1 && sym < 0.3, $"expected ~0.2 deviation, got {sym}");
        }

        [TestMethod]
        public void TestLscmUv()
        {
            // A flat grid is a clean disk (single boundary) — the canonical LSCM input.
            (Point3D[] v, MeshFace[] f) = MeshMaker.Grid(8);

            (double u, double v)[] uv = MeshFunctions.LscmUv(v.ToList(), f.ToList());
            Assert.AreEqual(v.Length, uv.Length, "one UV per vertex");
            Assert.IsTrue(uv.All(p => !double.IsNaN(p.u) && !double.IsNaN(p.v)), "UVs must be finite");
            // The map must be non-degenerate: some spread in both axes.
            double du = uv.Max(p => p.u) - uv.Min(p => p.u);
            double dv = uv.Max(p => p.v) - uv.Min(p => p.v);
            Assert.IsTrue(du > 1e-6 && dv > 1e-6, $"UV layout collapsed (du={du}, dv={dv})");
        }
    }
}
