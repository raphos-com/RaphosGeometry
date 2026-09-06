using Raphos.Geometry.Interop;
using Synera.Core.Graph.Data;
using Synera.Core.Graph.Enums;
using Synera.Core.Implementation.Graph;
using Synera.DataTypes;
using Synera.Kernels.DataTypes;
using Synera.Kernels.Mesh;
using Synera.Localization;
using System;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;

namespace Raphos.Geometry.Components.Parameterization
{
    [Guid("e91c23e7-6416-4c2e-a93a-d0035dbff43d")]
    public sealed class AutoUvAtlas : Node
    {
        public AutoUvAtlas()
            : base(new LocalizableString("Auto UV Atlas"))
        {
            Category = Shared.RaphosGeometryCategory;
            Subcategory = Shared.Mesh;
            Keywords = new LocalizableString("uv atlas chart segmentation pack seam texture unwrap");
            Description = new LocalizableString(
                "Segment a mesh into charts along sharp edges and flatten + pack them (Geogram atlas). "
                + "Outputs one UV per face-corner (three per triangle, in face order) so seams are preserved.");
            GuiPriority = 30;
            CanBeVisible = true;
            IsReadonly = false;

            InputParameterManager.AddParameter<IMesh>("Mesh", "Triangle mesh to atlas.", ParameterAccess.Item);
            InputParameterManager.AddParameter<SyneraDouble>(
                "Hard Angle", "Dihedral angle (degrees) above which a chart boundary is forced.",
                ParameterAccess.Item, new SyneraDouble(45.0));
            InputParameterManager.AddParameter<SyneraInt>(
                "Checker Squares", "Checkerboard frequency used to paint the UVs back onto the model.",
                ParameterAccess.Item, new SyneraInt(12));

            OutputParameterManager.AddParameter<Point3D>(
                new LocalizableString("UV"),
                new LocalizableString("UV per face-corner as an XY-plane point (3 per triangle, in face order)."),
                ParameterAccess.List);
            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Atlas Mesh"),
                new LocalizableString(
                    "The packed UV atlas as a flat mesh in the XY plane: preview it to see the charts "
                    + "you would bake a texture into. Same triangles as the input, laid out in 2D."),
                ParameterAccess.Item);
            OutputParameterManager.AddParameter<IMesh>(
                new LocalizableString("Textured Mesh"),
                new LocalizableString(
                    "The original 3D mesh with a checkerboard painted through the UVs — the round trip. "
                    + "Even squares mean a low-distortion unwrap; stretched squares reveal distortion."),
                ParameterAccess.Item);
        }

        protected override void SolveInstance(IDataAccess dataAccess)
        {
            if (!dataAccess.GetData(0, out IMesh m) |
                !dataAccess.GetData(1, out double hardAngle) |
                !dataAccess.GetData(2, out int checkerSquares))
                return;

            if (m.QuadCount > 0) { AddError(0, $"The mesh must contain triangles only; it has {m.QuadCount} quads."); return; }

            (double u, double v)[] uv = MeshFunctions.AutoUvAtlas(m.Vertices.ToList(), m.Faces.ToList(), hardAngle);
            if (uv.Length == 0)
            {
                AddWarning("The atlas produced no UVs.");
                return;
            }
            Point3D[] uvPts = uv.Select(p => new Point3D(p.u, p.v, 0)).ToArray();
            dataAccess.SetListData(0, uvPts);

            // The UVs are one point per face-corner in face order, so consecutive triples are
            // exactly the triangles: rebuild the packed atlas as a flat mesh you can preview.
            int faceCount = uvPts.Length / 3;
            var atlasFaces = new MeshFace[faceCount];
            for (int i = 0; i < faceCount; i++)
                atlasFaces[i] = new MeshFace(i * 3, i * 3 + 1, i * 3 + 2);
            dataAccess.SetData(1, MeshKernel.CreateFromVerticesAndFaces(uvPts, atlasFaces));

            // Map back: paint a checkerboard through the UVs onto the ORIGINAL 3D mesh, so you can
            // see the parameterization as a texture on the model (and spot distortion). Each face's
            // checker cell comes from the average UV of its three corners.
            int n = Math.Max(1, checkerSquares);
            var faceColors = new Color[faceCount];
            for (int f = 0; f < faceCount; f++)
            {
                double cu = (uv[f * 3].u + uv[f * 3 + 1].u + uv[f * 3 + 2].u) / 3.0;
                double cv = (uv[f * 3].v + uv[f * 3 + 1].v + uv[f * 3 + 2].v) / 3.0;
                int cell = (int)Math.Floor(cu * n) + (int)Math.Floor(cv * n);
                bool dark = (cell & 1) == 0;
                faceColors[f] = dark ? Color.FromArgb(40, 40, 46) : Color.FromArgb(225, 225, 232);
            }
            IMesh textured = MeshKernel.CreateFromVerticesAndFaces(m.Vertices.ToArray(), m.Faces.ToArray());
            textured.SetFaceColors(faceColors);
            dataAccess.SetData(2, textured);
        }
    }
}
