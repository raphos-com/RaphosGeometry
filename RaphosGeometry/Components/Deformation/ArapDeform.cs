using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Deformation
{
    [Guid("6637c466-4c80-4c93-99ef-48b0f872e6c6")]
    public sealed class ArapDeform : Node
    {
        public ArapDeform()
            : base(new LocalizableString("ARAP Deformation"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("arap deform handle as-rigid-as-possible drag pose morph");
            Description = new LocalizableString(
                "As-rigid-as-possible handle-based deformation: the nearest vertex to each handle point is "
                + "constrained to the corresponding target position and the mesh follows as rigidly as possible.");
            GuiPriority = 30;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Mesh to deform.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Point3D>("Handles", "Handle points (mapped to nearest vertices).", ParameterAccess.List);
            InputParameterManager.AddParameter<Point3D>("Targets", "Target position for each handle (same count/order).", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>("Iterations", "ARAP iterations.", ParameterAccess.Item, new SyneraInt(100));

            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Mesh"), new LocalizableString("Deformed mesh."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetListData(1, out IList<Point3D> handles) |
                !dataAccess.GetListData(2, out IList<Point3D> targets) |
                !dataAccess.GetData(3, out int iterations))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }
            if (handles == null || targets == null || handles.Count == 0 || handles.Count != targets.Count)
            {
                AddError(new int[] { 1, 2 }, "Provide matching non-empty Handles and Targets lists.");
                return;
            }

            int[] handleIdx = handles.Select(p => m.VertexTree.QueryNearest(p)).ToArray();
            Point3D[] deformed = MeshFunctions.ArapDeform(
                m.Vertices.ToList(), m.Faces.ToList(), handleIdx, targets, iterations);
            dataAccess.SetData(0, MeshKernel.CreateFromVerticesAndFaces(deformed, m.Faces.ToArray()));
        }
    }
}
