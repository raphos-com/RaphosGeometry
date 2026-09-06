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
    [Guid("bd443f58-9ae9-46cf-8991-d1d793820057")]
    public sealed class HausdorffDistance : Node
    {
        public HausdorffDistance()
            : base(new LocalizableString("Hausdorff Distance"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Analysis;
            Keywords = new LocalizableString("hausdorff deviation distance compare maximum error");
            Description = new LocalizableString(
                "Maximum (bounded) deviation between two meshes: directed A→B, B→A and the symmetric "
                + "Hausdorff distance. Complements min-distance nodes by reporting the worst-case gap.");
            GuiPriority = 30;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh A", "First mesh.", ParameterAccess.Item);
            InputParameterManager.AddParameter<IMesh>("Mesh B", "Second mesh.", ParameterAccess.Item);

            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("A to B"), new LocalizableString("Directed Hausdorff distance from A to B."), ParameterAccess.Item);
            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("B to A"), new LocalizableString("Directed Hausdorff distance from B to A."), ParameterAccess.Item);
            OutputParameterManager.AddParameter<SyneraDouble>(
                new LocalizableString("Symmetric"), new LocalizableString("Symmetric Hausdorff distance = max(A→B, B→A)."), ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh a) |
                !dataAccess.GetData(1, out IMesh b))
                return;

            (double aToB, double bToA, double sym) = MeshFunctions.HausdorffDistance(
                a.Vertices.ToList(), a.Faces.ToList(), b.Vertices.ToList(), b.Faces.ToList());
            dataAccess.SetData(0, aToB);
            dataAccess.SetData(1, bToA);
            dataAccess.SetData(2, sym);
        }
    }
}
