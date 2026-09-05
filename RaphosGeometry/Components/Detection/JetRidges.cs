using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Detection
{
    [Guid("ee22f448-10a3-4603-a115-67d7f95aa670")]
    public sealed class JetRidges : Node
    {
        public JetRidges()
            : base(new LocalizableString("Jet Ridges"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.PointCloud;
            Keywords = new LocalizableString("jet fitting ridges curvature monge point cloud feature");
            Description = new LocalizableString(
                "Per-point principal curvatures via Cazals-Pouget jet (Monge quadric) fitting on a point cloud. "
                + "Outputs k1/k2 and a ridge strength (|k1|) that highlights sharp feature lines.");
            GuiPriority = 30; CanBeVisible = true; IsReadonly = false;

            InputParameterManager.AddParameter<Point3D>("Points", "Input point cloud.", ParameterAccess.List);
            InputParameterManager.AddParameter<SyneraInt>("Neighbours", "Neighbours used for the local fit.", ParameterAccess.Item, new SyneraInt(18));

            OutputParameterManager.AddParameter<SyneraDouble>(new LocalizableString("Curvature 1"), new LocalizableString("Max principal curvature k1."), ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraDouble>(new LocalizableString("Curvature 2"), new LocalizableString("Min principal curvature k2."), ParameterAccess.List);
            OutputParameterManager.AddParameter<SyneraDouble>(new LocalizableString("Ridge Strength"), new LocalizableString("|k1| — high on sharp ridges."), ParameterAccess.List);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetListData(0, out IList<Point3D> pnts) |
                !dataAccess.GetData(1, out int k))
                return;
            if (pnts == null || pnts.Count < 6) { AddError(0, "Provide at least six points."); return; }

            (double[] k1, double[] k2) = MeshFunctions.JetCurvature(pnts, k);
            dataAccess.SetListData(0, k1);
            dataAccess.SetListData(1, k2);
            dataAccess.SetListData(2, k1.Select(Math.Abs).ToArray());
        }
    }
}
