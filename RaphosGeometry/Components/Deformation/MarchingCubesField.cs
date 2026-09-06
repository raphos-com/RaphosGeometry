using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Kernels.SpatialFields;
using Synera.Localization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Deformation
{
    /// <summary>
    /// Marching cubes driven by a Synera spatial field (<see cref="ISpatialField"/>): give a field
    /// (e.g. a distance field, or Raphos Tools' Surface Curvature Field), a box to sample it over and
    /// the grid resolution, and the node samples the field on the grid and meshes the level set. This
    /// is the "field in, surface out" form — no need to build the sample values yourself.
    /// </summary>
    [Guid("31b9b83d-0e2f-45cc-a250-151121e40eeb")]
    public sealed class MarchingCubesField : Node
    {
        public MarchingCubesField()
            : base(new LocalizableString("Marching Cubes (Field)"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("marching cubes isosurface field level set implicit sdf distance curvature domain box");
            Description = new LocalizableString(
                "Extract an isosurface from a Synera spatial field. Feed it any field (a distance field, "
                + "a curvature field, an FEA result field), the box to sample it over and the grid resolution; "
                + "the node samples the field's magnitude on the grid and meshes the chosen level set.");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<ISpatialField>(
                "Field", "Spatial field to sample (distance, curvature, result, …).", ParameterAccess.Item);
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
                "Isovalue", "Field magnitude to extract the surface at.", ParameterAccess.Item, new SyneraDouble(0.0));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"),
                new LocalizableString("Extracted isosurface triangle mesh."),
                ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out ISpatialField field) |
                !dataAccess.GetData(1, out Point3D lo) |
                !dataAccess.GetData(2, out Point3D hi) |
                !dataAccess.GetData(3, out int nx) |
                !dataAccess.GetData(4, out int ny) |
                !dataAccess.GetData(5, out int nz) |
                !dataAccess.GetData(6, out double iso))
                return;

            if (field == null) { AddError(0, "Provide a spatial field to sample."); return; }
            if (nx < 2 || ny < 2 || nz < 2) { AddError("Each of Nx, Ny, Nz must be at least 2."); return; }

            // Build the regular grid (x-fastest, then y, then z).
            int count = nx * ny * nz;
            var grid = new Point3D[count];
            int idx = 0;
            for (int k = 0; k < nz; k++)
            {
                double z = lo.Z + (nz > 1 ? (double)k / (nz - 1) : 0.0) * (hi.Z - lo.Z);
                for (int j = 0; j < ny; j++)
                {
                    double y = lo.Y + (ny > 1 ? (double)j / (ny - 1) : 0.0) * (hi.Y - lo.Y);
                    for (int i = 0; i < nx; i++)
                    {
                        double x = lo.X + (nx > 1 ? (double)i / (nx - 1) : 0.0) * (hi.X - lo.X);
                        grid[idx++] = new Point3D(x, y, z);
                    }
                }
            }

            // Sample the field over the grid; use the field vector's magnitude as the scalar value.
            FieldVector[] fv = field.EvaluateSparsely(grid, new Synera.Utilities.Progress(), out int[] _);
            var values = new double[count];
            for (int i = 0; i < count; i++)
                values[i] = fv[i].Length();

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
