using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Deformation
{
    /// <summary>
    /// Marching cubes driven by a scalar field over a box domain: you give the domain corners, the
    /// grid resolution and one value per grid sample, and the node builds the regular grid itself and
    /// extracts the isosurface. This is the "field in, surface out" form — no need to also supply the
    /// grid point coordinates the way <see cref="MarchingCubes"/> does.
    /// </summary>
    [Guid("31b9b83d-0e2f-45cc-a250-151121e40eeb")]
    public sealed class MarchingCubesField : Node
    {
        public MarchingCubesField()
            : base(new LocalizableString("Marching Cubes (Field)"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("marching cubes isosurface field level set implicit sdf gyroid domain box");
            Description = new LocalizableString(
                "Extract an isosurface from a scalar field sampled on a box. Give the domain corners, the "
                + "grid resolution (Nx*Ny*Nz) and one field value per sample in x-fastest, then y, then z order; "
                + "the node builds the grid and meshes the level set. Use this when you have field values but not "
                + "the grid coordinates.");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>(
                "Min Corner", "Minimum corner of the box the field is sampled over.",
                ParameterAccess.Item, new Point3D(0, 0, 0));
            InputParameterManager.AddParameter<Point3D>(
                "Max Corner", "Maximum corner of the box the field is sampled over.",
                ParameterAccess.Item, new Point3D(1, 1, 1));
            InputParameterManager.AddParameter<SyneraInt>(
                "Nx", "Number of grid samples along X.", ParameterAccess.Item, new SyneraInt(2));
            InputParameterManager.AddParameter<SyneraInt>(
                "Ny", "Number of grid samples along Y.", ParameterAccess.Item, new SyneraInt(2));
            InputParameterManager.AddParameter<SyneraInt>(
                "Nz", "Number of grid samples along Z.", ParameterAccess.Item, new SyneraInt(2));
            InputParameterManager.AddParameter<SyneraDouble>(
                "Values", "Field value at each grid sample (Nx*Ny*Nz), x-fastest then y then z order.",
                ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraDouble>(
                "Isovalue", "Field level to extract the surface at.", ParameterAccess.Item, new SyneraDouble(0.0));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"),
                new LocalizableString("Extracted isosurface triangle mesh."),
                ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out Point3D lo) |
                !dataAccess.GetData(1, out Point3D hi) |
                !dataAccess.GetData(2, out int nx) |
                !dataAccess.GetData(3, out int ny) |
                !dataAccess.GetData(4, out int nz) |
                !dataAccess.GetListData(5, out IList<double> values) |
                !dataAccess.GetData(6, out double iso))
                return;

            if (nx < 2 || ny < 2 || nz < 2)
            {
                AddError("Each of Nx, Ny, Nz must be at least 2.");
                return;
            }

            long expected = (long)nx * ny * nz;
            if (values.Count != expected)
            {
                AddError(5, $"Values ({values.Count}) must equal Nx*Ny*Nz ({expected}).");
                return;
            }

            // Build the regular grid the field is sampled on (x-fastest, then y, then z).
            var grid = new Point3D[expected];
            int idx = 0;
            for (int k = 0; k < nz; k++)
            {
                double tz = nz > 1 ? (double)k / (nz - 1) : 0.0;
                double z = lo.Z + tz * (hi.Z - lo.Z);
                for (int j = 0; j < ny; j++)
                {
                    double ty = ny > 1 ? (double)j / (ny - 1) : 0.0;
                    double y = lo.Y + ty * (hi.Y - lo.Y);
                    for (int i = 0; i < nx; i++)
                    {
                        double tx = nx > 1 ? (double)i / (nx - 1) : 0.0;
                        double x = lo.X + tx * (hi.X - lo.X);
                        grid[idx++] = new Point3D(x, y, z);
                    }
                }
            }

            (Point3D[] points, MeshFace[] faces) = MeshFunctions.MarchingCubes(values, grid, nx, ny, nz, iso);
            if (points.Length == 0)
            {
                AddWarning("The isosurface is empty at this isovalue.");
                return;
            }
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(points, faces));
        }
    }
}
