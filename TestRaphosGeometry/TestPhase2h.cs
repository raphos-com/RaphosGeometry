using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2h
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        [TestMethod]
        public void TestAlphaShape()
        {
            // Points sampled on a unit sphere. With a generous alpha the alpha shape is the full
            // (closed) boundary surface of the Delaunay tetrahedralization.
            (Point3D[] v, MeshFace[] _) = MeshMaker.Icosphere(1.0, 2);
            MeshFace[] faces = MeshFunctions.AlphaShape(v.ToList(), 2.0);

            Assert.IsTrue(faces.Length > 0, "alpha shape should produce faces");
            Assert.IsTrue(faces.All(f => f.A >= 0 && f.A < v.Length && f.B < v.Length && f.C < v.Length),
                "faces must index the input points");
        }

        [TestMethod]
        public void TestVectorHeat()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 3);
            // Transport an arbitrary tangent-ish direction from vertex 0.
            Vector3D[] vecs = MeshFunctions.VectorHeatTransport(v.ToList(), f.ToList(), 0, new Vector3D(1, 0, 0));

            Assert.AreEqual(v.Length, vecs.Length);
            // Parallel transport preserves magnitude: all transported vectors share ~the same length.
            double[] lens = vecs.Select(w => Math.Sqrt(w.X * w.X + w.Y * w.Y + w.Z * w.Z)).ToArray();
            double mean = lens.Average();
            Assert.IsTrue(mean > 1e-6, "transported vectors should be non-zero");
            Assert.IsTrue(lens.All(l => Math.Abs(l - mean) < 0.25 * mean), "transport should roughly preserve magnitude");
            // On a sphere the transported field should stay tangent: small radial component on average.
            double meanRadial = 0;
            for (int i = 0; i < v.Length; i++)
            {
                double len = Math.Sqrt(v[i].X * v[i].X + v[i].Y * v[i].Y + v[i].Z * v[i].Z);
                double radial = (v[i].X / len) * vecs[i].X + (v[i].Y / len) * vecs[i].Y + (v[i].Z / len) * vecs[i].Z;
                meanRadial += Math.Abs(radial);
            }
            meanRadial /= v.Length;
            Assert.IsTrue(meanRadial < 0.3 * mean, $"transported field should stay ~tangent, mean |radial|={meanRadial}");
        }
    }
}
