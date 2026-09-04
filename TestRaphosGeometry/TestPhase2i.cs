using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2i
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        [TestMethod]
        public void TestPoissonReconstruct()
        {
            // Oriented point cloud on a unit sphere.
            (Point3D[] v, MeshFace[] _) = MeshMaker.Icosphere(1.0, 4);
            var pnts = v.ToList();
            var normals = v.Select(p =>
            {
                double len = Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
                return new Vector3D(p.X / len, p.Y / len, p.Z / len);
            }).ToList();

            (Point3D[] ov, MeshFace[] of) = MeshFunctions.PoissonReconstruct(pnts, normals, 6);
            Assert.IsTrue(ov.Length > 0 && of.Length > 0, "Poisson should produce a surface");
            // The reconstructed watertight surface should approximate the unit sphere.
            double meanR = ov.Select(p => Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z)).Average();
            Assert.IsTrue(Math.Abs(meanR - 1.0) < 0.25, $"reconstructed radius {meanR}, expected ~1");
        }

        [TestMethod]
        public void TestAutoUvAtlas()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 3);
            (double u, double v)[] uv = MeshFunctions.AutoUvAtlas(v.ToList(), f.ToList(), 45.0);

            Assert.AreEqual(f.Length * 3, uv.Length, "expected one UV per face-corner");
            Assert.IsTrue(uv.All(p => !double.IsNaN(p.u) && !double.IsNaN(p.v)), "UVs must be finite");
            double du = uv.Max(p => p.u) - uv.Min(p => p.u);
            double dv = uv.Max(p => p.v) - uv.Min(p => p.v);
            Assert.IsTrue(du > 1e-6 && dv > 1e-6, $"atlas UV layout collapsed (du={du}, dv={dv})");
        }
    }
}
