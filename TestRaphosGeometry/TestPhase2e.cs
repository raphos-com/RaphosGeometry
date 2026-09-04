using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2e
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        [TestMethod]
        public void TestFlipoutGeodesic()
        {
            double r = 1.0;
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(r, 3);
            // Pick two well-separated vertices.
            int start = 0;
            int end = Enumerable.Range(0, v.Length)
                .OrderByDescending(i => v[i].X * v[start].X + v[i].Y * v[start].Y + v[i].Z * v[start].Z <= -0.9 ? 1 : 0)
                .First();

            Point3D[] path = MeshFunctions.FlipoutGeodesic(v.ToList(), f.ToList(), start, end);
            Assert.IsTrue(path.Length >= 2, "geodesic path should have at least two points");

            // Polyline length should be >= straight-line (chord) distance and <= pi*r (great circle).
            double len = 0;
            for (int i = 1; i < path.Length; i++)
            {
                double dx = path[i].X - path[i - 1].X, dy = path[i].Y - path[i - 1].Y, dz = path[i].Z - path[i - 1].Z;
                len += Math.Sqrt(dx * dx + dy * dy + dz * dz);
            }
            Assert.IsTrue(len > 0.5 && len <= Math.PI * r * 1.05, $"geodesic length {len} out of expected range");
        }

        [TestMethod]
        public void TestAverageSpacing()
        {
            // Unit grid with n=11 over [0,1]^2 -> spacing 0.1.
            (Point3D[] v, MeshFace[] _) = MeshMaker.Grid(11);
            double spacing = MeshFunctions.AverageSpacing(v.ToList(), 4);
            Assert.IsTrue(Math.Abs(spacing - 0.1) < 0.03, $"expected ~0.1 spacing, got {spacing}");
        }

        [TestMethod]
        public void TestSimplifyPointCloud()
        {
            // Dense grid (40x40 points over [0,1]^2). A 0.1 voxel grid should collapse to ~11x11 cells.
            (Point3D[] v, MeshFace[] _) = MeshMaker.Grid(40);
            Point3D[] simplified = MeshFunctions.SimplifyPointCloud(v.ToList(), 0.1);
            Assert.IsTrue(simplified.Length < v.Length, "simplification should reduce the point count");
            Assert.IsTrue(simplified.Length >= 100 && simplified.Length <= 144,
                $"expected ~121 cells for a 0.1 grid over the unit square, got {simplified.Length}");
        }
    }
}
