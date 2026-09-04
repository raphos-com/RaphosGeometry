using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase34
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        static List<Point3D> NoisySphere(double r, int sub, double noise, out List<Vector3D> normals, int seed = 7)
        {
            (Point3D[] v, MeshFace[] _) = MeshMaker.Icosphere(r, sub);
            var rng = new Random(seed);
            var pts = new List<Point3D>(); normals = new List<Vector3D>();
            foreach (var p in v)
            {
                double len = Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z);
                var n = new Vector3D(p.X / len, p.Y / len, p.Z / len);
                double d = (rng.NextDouble() - 0.5) * 2 * noise;
                pts.Add(new Point3D(p.X + n.X * d, p.Y + n.Y * d, p.Z + n.Z * d));
                normals.Add(n);
            }
            return pts;
        }

        [TestMethod]
        public void TestRansacDetect()
        {
            // A flat grid of points -> one plane, (almost) all points assigned.
            (Point3D[] v, MeshFace[] _) = MeshMaker.Grid(12);
            (int[] labels, int[] types) = MeshFunctions.RansacDetect(v.ToList(), 0.02, 20, 200);
            Assert.AreEqual(v.Length, labels.Length);
            Assert.IsTrue(types.Length >= 1, "should detect at least one primitive");
            int assigned = labels.Count(l => l >= 0);
            Assert.IsTrue(assigned > v.Length * 0.7, $"most points should be on the plane, got {assigned}/{v.Length}");
        }

        [TestMethod]
        public void TestRegionGrowing()
        {
            (Point3D[] v, MeshFace[] _) = MeshMaker.Grid(10);   // one smooth (flat) region
            (int[] labels, int regions) = MeshFunctions.RegionGrowing(v.ToList(), 15.0, 12, 5);
            Assert.AreEqual(v.Length, labels.Length);
            Assert.IsTrue(regions >= 1, "flat grid should form at least one region");
            Assert.IsTrue(labels.Count(l => l == 0) > v.Length * 0.7, "most points should share the first region");
        }

        [TestMethod]
        public void TestJetRidges()
        {
            (Point3D[] v, MeshFace[] _) = MeshMaker.Icosphere(2.0, 3);   // curvature 1/r = 0.5
            (double[] k1, double[] k2) = MeshFunctions.JetCurvature(v.ToList(), 18);
            Assert.AreEqual(v.Length, k1.Length);
            double med = k1.Select(Math.Abs).OrderBy(x => x).ElementAt(k1.Length / 2);
            Assert.IsTrue(med > 0.3 && med < 0.75, $"median |k1| should be ~0.5, got {med}");
        }

        [TestMethod]
        public void TestWlopConsolidate()
        {
            var pts = NoisySphere(1.0, 3, 0.05, out _);
            Point3D[] outp = MeshFunctions.WlopConsolidate(pts, 8, 0.3);
            Assert.AreEqual(pts.Count, outp.Length);
            double meanR = outp.Select(p => Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z)).Average();
            Assert.IsTrue(Math.Abs(meanR - 1.0) < 0.12, $"consolidated points should sit ~on the sphere, meanR={meanR}");
        }

        [TestMethod]
        public void TestBilateralDenoise()
        {
            // Bilateral is feature-preserving; with a wide normal sigma it also smooths noise.
            var pts = NoisySphere(1.0, 3, 0.05, out var normals);
            Point3D[] outp = MeshFunctions.BilateralDenoise(pts, normals, 0.15, 0.3, 3);
            Assert.AreEqual(pts.Count, outp.Length);
            // Result must stay a well-behaved cloud close to the sphere (marshalling + stability check).
            double meanR = outp.Select(p => Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z)).Average();
            Assert.IsTrue(meanR > 0.9 && meanR < 1.1, $"denoised points should remain near the sphere, meanR={meanR}");
            Assert.IsTrue(outp.All(p => !double.IsNaN(p.X)), "positions must be finite");
        }

        [TestMethod]
        public void TestMeanCurvatureSkeleton()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 3);
            Point3D[] c = MeshFunctions.MeanCurvatureSkeleton(v.ToList(), f.ToList(), 8, 0.2);
            Assert.AreEqual(v.Length, c.Length);
            // Mean-curvature flow shrinks a sphere: contracted points should be closer to the centre.
            double meanR = c.Select(p => Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z)).Average();
            Assert.IsTrue(meanR < 0.99, $"contraction should shrink the sphere, meanR={meanR}");
        }

        [TestMethod]
        public void TestAlphaWrap()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 3);
            (Point3D[] ov, MeshFace[] of) = MeshFunctions.AlphaWrap(v.ToList(), f.ToList(), 0.1, 40);
            Assert.IsTrue(ov.Length > 0 && of.Length > 0, "wrap should be non-empty");
            double meanR = ov.Select(p => Math.Sqrt(p.X * p.X + p.Y * p.Y + p.Z * p.Z)).Average();
            Assert.IsTrue(meanR > 0.9 && meanR < 1.5, $"wrap should envelope the sphere, meanR={meanR}");
        }

        [TestMethod]
        public void TestSdfSegmentation()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 3);
            (double[] sdf, int[] labels) = MeshFunctions.SdfSegmentation(v.ToList(), f.ToList(), 2);
            Assert.AreEqual(f.Length, sdf.Length);
            Assert.AreEqual(f.Length, labels.Length);
            // SDF (interior chord length) of a unit sphere is ~diameter = 2; all positive.
            double meanSdf = sdf.Where(x => x > 0).DefaultIfEmpty(0).Average();
            Assert.IsTrue(meanSdf > 1.0 && meanSdf < 2.5, $"sphere SDF should be ~2 (diameter), got {meanSdf}");
        }

        [TestMethod]
        public void TestAdvancingFront()
        {
            (Point3D[] v, MeshFace[] _) = MeshMaker.Icosphere(1.0, 3);
            MeshFace[] faces = MeshFunctions.AdvancingFront(v.ToList(), 0.0);
            Assert.IsTrue(faces.Length > 0, "reconstruction should produce triangles");
            Assert.IsTrue(faces.All(t => t.A >= 0 && t.A < v.Length && t.B < v.Length && t.C < v.Length),
                "faces must index the input points");
        }
    }
}
