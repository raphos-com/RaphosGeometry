using System.Runtime.InteropServices;

namespace Raphos.Geometry.Interop
{
    /// <summary>
    /// Managed mirror of the native MeshFromPointCloudConfig struct (Co3Ne reconstruction).
    /// Field order and types must match the C++ definition exactly.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    public struct MeshFromPointCloudConfig
    {
        public double radius = 5;
        public long Psmooth_iter = 0;
        public long nb_neighbors = 30;
        public bool repair = true;
        public double max_N_angle = 60.0;
        public double max_hole_area = 5;
        public long max_hole_edges = 500;
        public double min_comp_area = 0.01;
        public long min_comp_facets = 10;

        public MeshFromPointCloudConfig() { }
    }
}
