using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2j
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

        [TestMethod]
        public void TestBiharmonicWeights()
        {
            // Grid with two handles at opposite corners.
            (Point3D[] v, MeshFace[] f) = MeshMaker.Grid(9);
            var handles = new List<Point3D> { new Point3D(0, 0, 0), new Point3D(1, 1, 0) };

            double[][] w = MeshFunctions.BiharmonicWeights(v.ToList(), f.ToList(), handles);
            Assert.AreEqual(v.Length, w.Length);
            Assert.AreEqual(2, w[0].Length);

            // Partition of unity: each vertex's weights sum to ~1, and each weight is in [0,1].
            foreach (var row in w)
            {
                Assert.AreEqual(1.0, row.Sum(), 1e-3, "weights should sum to 1");
                Assert.IsTrue(row.All(x => x > -1e-6 && x < 1 + 1e-6), "weights must be bounded in [0,1]");
            }

            // Handle 0 (corner 0,0) should dominate near its own corner and be small at the far corner.
            int corner0 = 0;               // (0,0)
            int corner1 = v.Length - 1;    // (1,1)
            Assert.IsTrue(w[corner0][0] > 0.8, $"handle 0 should dominate at its corner, got {w[corner0][0]}");
            Assert.IsTrue(w[corner1][0] < 0.2, $"handle 0 should be small at the far corner, got {w[corner1][0]}");
        }
    }
}
