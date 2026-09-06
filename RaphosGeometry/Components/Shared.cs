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

        // Subcategories. Kept to three so the ribbon stays compact (each subcategory is a
        // ribbon group). 2nd arg = sort order within the category; the nameof(...) key ties
        // each to Icons/RaphosGeometryCategory/<Sub>/*.svg and Help/RaphosGeometryCategory/<Sub>/...
        //   Mesh       — remeshing/repair, UV parameterization, deformation, marching cubes.
        //   Analysis   — curvature, geodesics, winding number, spectral/heat fields, distances.
        //   PointCloud — reconstruction, normals, denoise/simplify, and shape/feature detection.
        // Within a subcategory, node GuiPriority is bucketed to exactly three values so the ribbon
        // shows at most three group dividers per subcategory (Synera splits a subcategory into
        // sub-groups by distinct GuiPriority). Convention: 10 / 20 / 30. Meaning per subcategory:
        //   Mesh:       10 cleanup & repair | 20 remesh & convert | 30 deform & UV
        //   Analysis:   10 fields & geodesics | 20 curvature & spectral | 30 distance & queries
        //   PointCloud: 10 preprocess | 20 reconstruction | 30 detection & segmentation
        public static readonly ICategory Test = new Category("Test", 0, nameof(Test).ToString());
        public static readonly ICategory Mesh = new Category("Mesh", 10, nameof(Mesh).ToString());
        public static readonly ICategory Analysis = new Category("Analysis", 20, nameof(Analysis).ToString());
        public static readonly ICategory PointCloud = new Category("Point Cloud", 30, nameof(PointCloud).ToString());
    }
}
