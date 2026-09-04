namespace Raphos.Geometry.Interop
{
    /// <summary>
    /// Thin managed wrappers over the trivial native test exports. Used by the
    /// Phase-0 round-trip node to validate P/Invoke marshalling end-to-end.
    /// </summary>
    public class UnsafeUtils
    {
        public static bool IsAllGood(bool b)
        {
            return UnsafeNativeMethods.IsAllGood(b);
        }

        public static double Sum(double a, double b)
        {
            UnsafeNativeMethods.Sum(a, b, out double result);
            return result;
        }
    }
}
