using Synera.Core.Implementation.UI;
using Synera.Core.UI;
using Synera.Localization;

namespace Raphos.Geometry.Components
{
    internal class Shared
    {
        // Top-level palette category. The sort key (175) places it just after Raphos Tools (170).
        public static readonly ICategory RaphosGeometryCategory = new Category(
            new LocalizableString("Raphos Geometry"),
            175,
            nameof(RaphosGeometryCategory),
            new LocalizableString("Raphos Geometry"),
            new LocalizableString("Research-grade mesh, surface and point-cloud processing."),
            typeof(Shared).Assembly);

        // Subcategories. 2nd arg = sort order within the category; the nameof(...) key ties
        // each to Icons/RaphosGeometryCategory/<Sub>/*.svg and Help/RaphosGeometryCategory/<Sub>/...
        public static readonly ICategory Test = new Category("Test", 0, nameof(Test).ToString());
        public static readonly ICategory Remeshing = new Category("Remeshing", 10, nameof(Remeshing).ToString());
        public static readonly ICategory PointCloud = new Category("Point Cloud", 20, nameof(PointCloud).ToString());
        public static readonly ICategory Analysis = new Category("Analysis", 30, nameof(Analysis).ToString());
        public static readonly ICategory Parameterization = new Category("Parameterization", 40, nameof(Parameterization).ToString());
        public static readonly ICategory Deformation = new Category("Deformation", 50, nameof(Deformation).ToString());
        public static readonly ICategory Detection = new Category("Detection", 60, nameof(Detection).ToString());
    }
}
