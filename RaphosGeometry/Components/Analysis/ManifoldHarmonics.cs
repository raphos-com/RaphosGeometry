using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Analysis
{
    [Guid("4b0ce07f-137e-415f-838d-a6f45ceac88e")]
    public sealed class ManifoldHarmonics : Node
    {
        public ManifoldHarmonics()
            : base(new LocalizableString("Manifold Harmonics"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Analysis;
            Keywords = new LocalizableString("manifold harmonics spectral laplacian eigenfunction eigenvalue fourier");
            Description = new LocalizableString(
                "Laplace-Beltrami eigenfunctions of a mesh (the spectral / 'manifold Fourier' basis). "
                + "Outputs the eigenvalues and one selected eigenfunction as a per-vertex field.");
            GuiPriority = 20;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Triangle mesh to analyse.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraInt>(
                "Count", "Number of eigenfunctions to compute.", ParameterAccess.Item, new SyneraInt(10));
            InputParameterManager.AddParameter<SyneraInt>(
                "Index", "Which eigenfunction (0-based) to output as a field.", ParameterAccess.Item, new SyneraInt(1));

            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Eigenvalues"), new LocalizableString("Computed eigenvalues (ascending)."), ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Eigenfunction"), new LocalizableString("Selected eigenfunction value per vertex."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out int count) |
                !dataAccess.GetData(2, out int index))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }

            (double[] eigenvalues, double[][] eigenfunctions) =
                MeshFunctions.ManifoldHarmonics(m.Vertices.ToList(), m.Faces.ToList(), count);

            if (eigenfunctions.Length == 0)
            {
                AddError(0, "No eigenfunctions were computed.");
                return;
            }
            if (index < 0 || index >= eigenfunctions.Length)
            {
                AddError(2, $"Index must be between 0 and {eigenfunctions.Length - 1}.");
                return;
            }

            dataAccess.SetListData(0, eigenvalues);
            dataAccess.SetListData(1, eigenfunctions[index]);
        }
    }
}
