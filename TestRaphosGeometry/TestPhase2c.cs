using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2c
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        [TestMethod]
        public void TestMeshFromPointCloud()
        {
            // Point cloud sampled on a unit sphere, with outward normals.
            (Point3D[] v, MeshFace[] _) = MeshMaker.Icosphere(1.0, 3);
            var pnts = v.ToList();
            var normals = v.Select(p =>
            {
                double len = Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
                return new Vector3D(p.X / len, p.Y / len, p.Z / len);
            }).ToList();

            var cfg = new MeshFromPointCloudConfig { radius = 0.3, nb_neighbors = 30 };
            (Point3D[] ov, MeshFace[] of) = MeshFunctions.MeshFromPointCloud(pnts, normals, cfg);

            Assert.IsTrue(of.Length > 0, "reconstruction should produce faces");
            Assert.IsTrue(ov.Length > 0);
            // Reconstructed vertices should sit on the unit sphere.
            double meanR = ov.Select(p => Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z)).Average();
            Assert.IsTrue(Math.Abs(meanR - 1.0) < 0.15, $"reconstructed points should be ~unit radius, got {meanR}");
        }

        [TestMethod]
        public void TestRemoveOutliers()
        {
            // Dense cluster near the origin plus a few far-flung outliers.
            var rng = new Random(1);
            var pnts = new List<Point3D>();
            for (int i = 0; i < 400; i++)
                pnts.Add(new Point3D(rng.NextDouble() * 0.1, rng.NextDouble() * 0.1, rng.NextDouble() * 0.1));
            int outliers = 5;
            for (int i = 0; i < outliers; i++)
                pnts.Add(new Point3D(10 + i, 10 + i, 10 + i));

            Point3D[] cleaned = MeshFunctions.CleanPointCloud(pnts, 20, 0.05);
            Assert.IsTrue(cleaned.Length <= pnts.Count - outliers, $"outliers should be removed: {pnts.Count} -> {cleaned.Length}");
            Assert.IsTrue(cleaned.Length > 300, "the dense cluster should largely survive");
            // No surviving point should be near the outlier locations.
            Assert.IsTrue(cleaned.All(p => p.X < 5), "far outliers must be gone");
        }
    }
}
