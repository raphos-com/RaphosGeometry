#if !RELEASE
using Raphos.Geometry.Components;
using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Localization;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Test
{
    // Phase-0 round-trip node: validates the native <-> interop P/Invoke path end to end.
    // Debug-only and hidden from the palette; it exists to prove the toolchain, not to ship.
    [Guid("b7d491a4-5bc9-4c79-a48a-f8197eeb21ba")]
    public sealed class Sum : Node
    {
        public Sum()
            : base(new LocalizableString("Test Sum"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Test;
            Keywords = "";
            Description = "Native round-trip smoke test.";
            GuiPriority = 10;
            CanBeVisible = false;
            IsReadonly = false;

            InputParameterManager.AddParameter<SyneraDouble>(
                "A", "A",
                ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraDouble>(
                "B", "B",
                ParameterAccess.Item);
            OutputParameterManager.AddParameter<SyneraDouble>(
                "C", "C",
                ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            dataAccess.GetData(0, out double a);
            dataAccess.GetData(1, out double b);

            var c = UnsafeUtils.Sum(a, b);
            dataAccess.SetData(0, c);
        }
    }
}
#endif
