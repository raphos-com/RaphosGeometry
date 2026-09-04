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
    [Guid("ea6ecace-8892-4705-98e5-228550d8ea2d")]
    public sealed class BiharmonicWeights : Node
    {
        public BiharmonicWeights()
            : base(new LocalizableString("Biharmonic Weights"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Deformation;
            Keywords = new LocalizableString("bbw bounded biharmonic weights skinning handles rig influence");
            Description = new LocalizableString(
                "Bounded biharmonic skinning weights for a set of point handles: smooth, non-negative and "
                + "partition-of-unity. Outputs the influence of one selected handle as a per-vertex field.");
            GuiPriority = 40;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Mesh to weight.", ParameterAccess.Item);
            InputParameterManager.AddParameter<Point3D>("Handles", "Control-point handles.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>(
                "Handle Index", "Which handle's weight field to output (0-based).", ParameterAccess.Item, new SyneraInt(0));

            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Weights"),
                new LocalizableString("Weight of the selected handle at each vertex (mesh vertex order)."),
                ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetListData(1, out IList<Point3D> handles) |
                !dataAccess.GetData(2, out int index))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }
            if (handles == null || handles.Count < 2)
            {
                AddError(1, "Provide at least two handles.");
                return;
            }

            double[][] weights = MeshFunctions.BiharmonicWeights(m.Vertices.ToList(), m.Faces.ToList(), handles);
            if (weights.Length == 0 || weights[0].Length == 0)
            {
                AddError(0, "Weight computation failed (check the mesh is a single connected component).");
                return;
            }
            if (index < 0 || index >= weights[0].Length)
            {
                AddError(2, $"Handle Index must be between 0 and {weights[0].Length - 1}.");
                return;
            }

            double[] field = weights.Select(w => w[index]).ToArray();
            dataAccess.SetListData(0, field);
        }
    }
}
