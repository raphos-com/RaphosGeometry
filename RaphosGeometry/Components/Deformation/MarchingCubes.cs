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
    [Guid("623b811c-4e4b-4595-a06a-f1c61fb9a175")]
    public sealed class MarchingCubes : Node
    {
        public MarchingCubes()
            : base(new LocalizableString("Marching Cubes"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("marching cubes isosurface contour implicit field level set");
            Description = new LocalizableString(
                "Extract an isosurface triangle mesh from a scalar field sampled on a regular grid. "
                + "Grid points and values must be listed in x-fastest, then y, then z order.");
            GuiPriority = 10;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>(
                "Grid Points", "Regular-grid sample positions (nx*ny*nz), x-fastest order.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraDouble>(
                "Values", "Scalar field value at each grid point (same order and count).", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>(
                "Nx", "Number of grid samples along X.", ParameterAccess.Item, new SyneraInt(2));
            InputParameterManager.AddParameter<SyneraInt>(
                "Ny", "Number of grid samples along Y.", ParameterAccess.Item, new SyneraInt(2));
            InputParameterManager.AddParameter<SyneraInt>(
                "Nz", "Number of grid samples along Z.", ParameterAccess.Item, new SyneraInt(2));
            InputParameterManager.AddParameter<SyneraDouble>(
                "Isovalue", "Field level to extract the surface at.", ParameterAccess.Item, new SyneraDouble(0.0));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"),
                new LocalizableString("Extracted isosurface triangle mesh."),
                ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> grid) |
                !dataAccess.GetListData(1, out IList<double> values) |
                !dataAccess.GetData(2, out int nx) |
                !dataAccess.GetData(3, out int ny) |
                !dataAccess.GetData(4, out int nz) |
                !dataAccess.GetData(5, out double iso))
                return;

            long expected = (long)nx * ny * nz;
            if (grid.Count != expected || values.Count != expected)
            {
                AddError($"Grid Points ({grid.Count}) and Values ({values.Count}) must each equal Nx*Ny*Nz ({expected}).");
                return;
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
