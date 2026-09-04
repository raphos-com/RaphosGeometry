using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2f
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        [TestMethod]
        public void TestManifoldHarmonics()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 3);
            (double[] vals, double[][] funcs) = MeshFunctions.ManifoldHarmonics(v.ToList(), f.ToList(), 8);

            Assert.IsTrue(vals.Length >= 4, $"expected several eigenvalues, got {vals.Length}");
            Assert.AreEqual(vals.Length, funcs.Length);
            Assert.AreEqual(v.Length, funcs[0].Length, "each eigenfunction has one value per vertex");

            // The first eigenvalue of the Laplacian is ~0 (constant eigenfunction); values are non-negative
            // and non-decreasing.
            Assert.IsTrue(Math.Abs(vals[0]) < 1e-3, $"first eigenvalue should be ~0, got {vals[0]}");
            for (int i = 1; i < vals.Length; i++)
                Assert.IsTrue(vals[i] >= vals[i - 1] - 1e-6, $"eigenvalues should be ascending at {i}: {vals[i - 1]} -> {vals[i]}");
        }

        [TestMethod]
        public void TestOrientNormals()
        {
            // Sphere point cloud with radial normals, but with random sign flips introduced.
            (Point3D[] v, MeshFace[] _) = MeshMaker.Icosphere(1.0, 3);
            var pnts = v.ToList();
            var rng = new Random(3);
            var flipped = v.Select(p =>
            {
                double len = Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
                double s = rng.NextDouble() < 0.5 ? -1 : 1;   // random sign
                return new Vector3D(s * p.X / len, s * p.Y / len, s * p.Z / len);
            }).ToList();

            Vector3D[] oriented = MeshFunctions.OrientNormals(pnts, flipped, 12);
            Assert.AreEqual(v.Length, oriented.Length);

            // After orientation, neighbouring normals should agree; on a sphere that means (almost) all
            // point outward (positive dot with the radial direction).
            int outward = 0;
            for (int i = 0; i < v.Length; i++)
            {
                double len = Math.Sqrt(v[i].X * v[i].X + v[i].Y * v[i].Y + v[i].Z * v[i].Z);
                double dot = (v[i].X / len) * oriented[i].X + (v[i].Y / len) * oriented[i].Y + (v[i].Z / len) * oriented[i].Z;
                if (dot > 0) outward++;
            }
            double frac = (double)outward / v.Length;
            Assert.IsTrue(frac > 0.95, $"expected consistently outward normals after orientation, got {frac:P0}");
        }
    }
}
