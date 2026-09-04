using Raphos.Geometry.Interop;
using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    [TestClass]
    public class TestPhase2
    {
        [TestCleanup]
        public void AfterEach() => GC.Collect();

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
        public void TestRepairMeshMergesDuplicateVertices()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 2);
            // Duplicate every vertex and re-index half the faces onto the copies, so the mesh
            // has coincident vertices that repair should weld back together.
            var doubled = v.Concat(v).ToList();
            int n = v.Length;
            var faces = new List<MeshFace>();
            for (int i = 0; i < f.Length; i++)
            {
                MeshFace face = f[i];
                if (i % 2 == 0) faces.Add(new MeshFace(face.A + n, face.B + n, face.C + n));
                else faces.Add(face);
            }

            (Point3D[] ov, MeshFace[] of) = MeshFunctions.RepairMesh(doubled, faces, 1e-9, true);
            Assert.IsTrue(ov.Length <= n + 1, $"expected ~{n} welded vertices, got {ov.Length}");
            Assert.AreEqual(0, BoundaryEdges(of), "welded sphere should be closed");
        }

        [TestMethod]
        public void TestMakeConsistentReorientsFlippedFaces()
        {
            (Point3D[] v, MeshFace[] f) = MeshMaker.Icosphere(1.0, 2);
            // Flip the winding of half the faces.
            var mixed = f.Select((face, i) => i % 2 == 0 ? new MeshFace(face.A, face.C, face.B) : face).ToArray();

            (Point3D[] ov, MeshFace[] of) = MeshFunctions.MakeConsistent(v.ToList(), mixed.ToList());
            Assert.AreEqual(0, BoundaryEdges(of), "closed sphere stays closed after reorienting");
            Assert.IsTrue(of.Length >= f.Length - 2, "faces should be preserved");

            // After reorientation every interior edge should be shared by two oppositely-directed
            // half-edges (consistent winding): no directed half-edge appears twice.
            var directed = new HashSet<(int, int)>();
            bool consistent = true;
            void Add(int a, int b) { if (!directed.Add((a, b))) consistent = false; }
            foreach (var face in of) { Add(face.A, face.B); Add(face.B, face.C); Add(face.C, face.A); }
            Assert.IsTrue(consistent, "orientation should be consistent (no repeated directed half-edge)");
        }

        [TestMethod]
        public void TestRemoveSelfIntersections()
        {
            // Two interpenetrating spheres form a self-intersecting mesh.
            (Point3D[] a, MeshFace[] fa) = MeshMaker.Icosphere(1.0, 2);
            (Point3D[] b, MeshFace[] fb) = MeshMaker.Icosphere(1.0, 2);
            var verts = a.Concat(b.Select(p => new Point3D(p.X + 1.2, p.Y, p.Z))).ToList();
            int n = a.Length;
            var faces = fa.Concat(fb.Select(f => new MeshFace(f.A + n, f.B + n, f.C + n))).ToList();

            (Point3D[] ov, MeshFace[] of) = MeshFunctions.RemoveSelfIntersections(verts, faces, 3);
            Assert.IsTrue(ov.Length > 0 && of.Length > 0, "result should be a valid non-empty mesh");
        }
    }
}
