using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2d
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        [TestMethod]
        public void TestHarmonicParam()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Grid(8);
            (double u, double v)[] uv = MeshFunctions.HarmonicParam(v.ToList(), f.ToList());

            Assert.AreEqual(v.Length, uv.Length);
            Assert.IsTrue(uv.All(p => !double.IsNaN(p.u) && !double.IsNaN(p.v)));
            double du = uv.Max(p => p.u) - uv.Min(p => p.u);
            double dv = uv.Max(p => p.v) - uv.Min(p => p.v);
            Assert.IsTrue(du > 1e-6 && dv > 1e-6, $"UV layout collapsed (du={du}, dv={dv})");
        }

        [TestMethod]
        public void TestArapUv()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Grid(8);
            (double u, double v)[] uv = MeshFunctions.ArapUv(v.ToList(), f.ToList(), 30);

            Assert.AreEqual(v.Length, uv.Length);
            Assert.IsTrue(uv.All(p => !double.IsNaN(p.u) && !double.IsNaN(p.v)));
            double du = uv.Max(p => p.u) - uv.Min(p => p.u);
            double dv = uv.Max(p => p.v) - uv.Min(p => p.v);
            Assert.IsTrue(du > 1e-6 && dv > 1e-6, $"ARAP UV collapsed (du={du}, dv={dv})");
        }

        [TestMethod]
        public void TestEstimateNormals()
        {
            // Points on a unit sphere: the PCA normal should be (anti)parallel to the radial direction.
            (Point3D[] v, MeshFace[] _) = MeshMaker.Icosphere(1.0, 3);
            Vector3D[] normals = MeshFunctions.EstimateNormals(v.ToList(), 16);

            Assert.AreEqual(v.Length, normals.Length);
            double meanAlign = 0;
            for (int i = 0; i < v.Length; i++)
            {
                double len = Math.Sqrt(v[i].X * v[i].X + v[i].Y * v[i].Y + v[i].Z * v[i].Z);
                double dot = (v[i].X / len) * normals[i].X + (v[i].Y / len) * normals[i].Y + (v[i].Z / len) * normals[i].Z;
                meanAlign += Math.Abs(dot);   // sign is not resolved by PCA
            }
            meanAlign /= v.Length;
            Assert.IsTrue(meanAlign > 0.9, $"estimated normals should align with the radial direction, mean |dot|={meanAlign}");
        }
    }
}
