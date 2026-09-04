using Synera.Kernels.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Interop
{
    /// <summary>
    /// Flatten managed geometry into interleaved unmanaged buffers (xyzxyz… for
    /// points, index-triples for faces) and unflatten native output buffers back.
    /// Buffers are allocated with Marshal.AllocHGlobal; the caller frees inputs and
    /// releases native outputs via ReleaseMemory* (see MeshFunctions ownership rules).
    /// </summary>
    internal class ArrayUtils
    {
        unsafe static void ArrayFromLongs(IEnumerable<long> vals, long* ptr, int l)
        {
            int i = 0;
            foreach (var item in vals)
            {
                ptr[i++] = item;
            }
        }

        unsafe static void ArrayFromPoints(IEnumerable<Point3D> pnts, double* ptr, int l)
        {
            int i = 0;
            foreach (var item in pnts)
            {
                ptr[i * 3 + 0] = item.X;
                ptr[i * 3 + 1] = item.Y;
                ptr[i * 3 + 2] = item.Z;
                i++;
            }
        }

        unsafe static void ArrayFromPoints(IEnumerable<Vector3D> vectors, double* ptr, int l)
        {
            int i = 0;
            foreach (var item in vectors)
            {
                ptr[i * 3 + 0] = item.X;
                ptr[i * 3 + 1] = item.Y;
                ptr[i * 3 + 2] = item.Z;
                i++;
            }
        }

        public static void ArrayFromPoints(IList<Point3D> pnts, out IntPtr arr, out int l)
        {
            l = pnts.Count * 3;
            arr = Marshal.AllocHGlobal(sizeof(double) * l);
            unsafe
            {
                double* ptr = (double*)arr.ToPointer();
                ArrayFromPoints(pnts, ptr, l);
            }
        }

        public static void ArrayFromPoints(IList<Vector3D> vectors, out IntPtr arr, out int l)
        {
            l = vectors.Count * 3;
            arr = Marshal.AllocHGlobal(sizeof(double) * l);
            unsafe
            {
                double* ptr = (double*)arr.ToPointer();
                ArrayFromPoints(vectors, ptr, l);
            }
        }

        public static void ArrayFromPoints(Point3D[] pnts, out IntPtr arr, out int l)
        {
            l = pnts.Length * 3;
            arr = Marshal.AllocHGlobal(sizeof(double) * l);
            unsafe
            {
                double* ptr = (double*)arr.ToPointer();
                ArrayFromPoints(pnts, ptr, l);
            }
        }

        public static void ArrayFromTriFaces(IList<MeshFace> faces, out IntPtr arr, out int l)
        {
            l = faces.Count * 3;
            arr = Marshal.AllocHGlobal(sizeof(long) * l);
            var faceIdxs = faces.SelectMany(f => new long[] { f.A, f.B, f.C });
            unsafe
            {
                long* ptr = (long*)arr.ToPointer();
                ArrayFromLongs(faceIdxs, ptr, l);
            }
        }

        public static void ArrayFromIntPtr<T>(IntPtr ptr, long l, out T[] arr)
            where T : struct
        {
            arr = new T[l];
            for (int i = 0; i < l; i++)
            {
                arr[i] = Marshal.PtrToStructure<T>(ptr);
                ptr = IntPtr.Add(ptr, Marshal.SizeOf(typeof(T)));
            }
        }
    }
}
