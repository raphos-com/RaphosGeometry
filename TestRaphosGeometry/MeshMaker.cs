using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using System;
using System.Collections.Generic;
using System.Linq;

namespace TestRaphosGeometry
{
    /// <summary>
    /// Synthetic geometry with analytic ground truth for validating the nodes.
    /// </summary>
    internal static class MeshMaker
    {
        /// <summary>
        /// Icosphere of the given radius, produced by recursively subdividing an icosahedron
        /// and projecting to the sphere. A sphere of radius r has principal curvatures 1/r
        /// everywhere and winding number 1 for interior points — handy analytic checks.
        /// </summary>
        public static (Point3D[] verts, MeshFace[] faces) Icosphere(double radius, int subdivisions)
        {
            double t = (1.0 + Math.Sqrt(5.0)) / 2.0;
            var v = new List<double[]>
            {
                new[]{-1.0, t, 0}, new[]{ 1.0, t, 0}, new[]{-1.0,-t, 0}, new[]{ 1.0,-t, 0},
                new[]{ 0.0,-1, t}, new[]{ 0.0, 1, t}, new[]{ 0.0,-1,-t}, new[]{ 0.0, 1,-t},
                new[]{ t, 0.0,-1}, new[]{ t, 0.0, 1}, new[]{-t, 0.0,-1}, new[]{-t, 0.0, 1},
            };
            var f = new List<int[]>
            {
                new[]{0,11,5}, new[]{0,5,1}, new[]{0,1,7}, new[]{0,7,10}, new[]{0,10,11},
                new[]{1,5,9}, new[]{5,11,4}, new[]{11,10,2}, new[]{10,7,6}, new[]{7,1,8},
                new[]{3,9,4}, new[]{3,4,2}, new[]{3,2,6}, new[]{3,6,8}, new[]{3,8,9},
                new[]{4,9,5}, new[]{2,4,11}, new[]{6,2,10}, new[]{8,6,7}, new[]{9,8,1},
            };

            var midCache = new Dictionary<long, int>();
            int Midpoint(int a, int b)
            {
                long key = a < b ? ((long)a << 32) | (uint)b : ((long)b << 32) | (uint)a;
                if (midCache.TryGetValue(key, out int idx)) return idx;
                var pa = v[a]; var pb = v[b];
                v.Add(new[] { (pa[0] + pb[0]) / 2, (pa[1] + pb[1]) / 2, (pa[2] + pb[2]) / 2 });
                idx = v.Count - 1;
                midCache[key] = idx;
                return idx;
            }

            for (int s = 0; s < subdivisions; s++)
            {
                var nf = new List<int[]>(f.Count * 4);
                foreach (var tri in f)
                {
                    int a = Midpoint(tri[0], tri[1]);
                    int b = Midpoint(tri[1], tri[2]);
                    int c = Midpoint(tri[2], tri[0]);
                    nf.Add(new[] { tri[0], a, c });
                    nf.Add(new[] { tri[1], b, a });
                    nf.Add(new[] { tri[2], c, b });
                    nf.Add(new[] { a, b, c });
                }
                f = nf;
            }

            var verts = v.Select(p =>
            {
                double len = Math.Sqrt(p[0] * p[0] + p[1] * p[1] + p[2] * p[2]);
                double k = radius / len;
                return new Point3D(p[0] * k, p[1] * k, p[2] * k);
            }).ToArray();
            var faces = f.Select(tri => new MeshFace(tri[0], tri[1], tri[2])).ToArray();
            return (verts, faces);
        }

        /// <summary>
        /// A flat triangulated grid in the XY plane spanning [0,1]x[0,1] with n*n vertices.
        /// Clean disk topology (a single boundary loop) — ideal input for UV unwrapping.
        /// </summary>
        public static (Point3D[] verts, MeshFace[] faces) Grid(int n)
        {
            var verts = new Point3D[n * n];
            for (int j = 0; j < n; j++)
                for (int i = 0; i < n; i++)
                    verts[j * n + i] = new Point3D((double)i / (n - 1), (double)j / (n - 1), 0);

            var faces = new List<MeshFace>((n - 1) * (n - 1) * 2);
            for (int j = 0; j < n - 1; j++)
                for (int i = 0; i < n - 1; i++)
                {
                    int a = j * n + i, b = j * n + i + 1, c = (j + 1) * n + i, d = (j + 1) * n + i + 1;
                    faces.Add(new MeshFace(a, b, d));
                    faces.Add(new MeshFace(a, d, c));
                }
            return (verts, faces.ToArray());
        }

        /// <summary>
        /// A regular nres^3 grid over [-half, half]^3 with a signed-distance field of a sphere
        /// (value = |p| - radius). Marching cubes at isovalue 0 should recover the sphere.
        /// Points are emitted x-fastest, then y, then z (libigl marching_cubes ordering).
        /// </summary>
        public static (List<Point3D> grid, List<double> values, int n) SphereSdfGrid(double radius, double half, int nres)
        {
            var grid = new List<Point3D>(nres * nres * nres);
            var values = new List<double>(nres * nres * nres);
            for (int k = 0; k < nres; k++)
                for (int j = 0; j < nres; j++)
                    for (int i = 0; i < nres; i++)
                    {
                        double x = -half + 2 * half * i / (nres - 1);
                        double y = -half + 2 * half * j / (nres - 1);
                        double z = -half + 2 * half * k / (nres - 1);
                        grid.Add(new Point3D(x, y, z));
                        values.Add(Math.Sqrt(x * x + y * y + z * z) - radius);
                    }
            return (grid, values, nres);
        }
    }
}
