using System;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Interop
{
    /// <summary>
    /// RAII helper around an unmanaged buffer allocated with AllocHGlobal.
    /// </summary>
    internal class IntPtrDisp : IDisposable
    {
        internal IntPtr _ptr;

        internal IntPtrDisp(IntPtr ptr)
        {
            this._ptr = ptr;
        }
        internal IntPtrDisp(int l, Type t)
        {
            _ptr = Marshal.AllocHGlobal(Marshal.SizeOf(t) * l);
        }

        public void Dispose()
        {
            if (_ptr != IntPtr.Zero)
                Marshal.FreeHGlobal(_ptr);
        }

        ~IntPtrDisp()
        {
            Dispose();
        }
    }
}
