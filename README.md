# Raphos Geometry

A standalone Synera add-in for research-grade triangle-mesh, surface and point-cloud
processing — the fine-grained mesh operations a Parasolid BREP kernel does not cover
(remeshing, decimation, geodesics, curvature tensors, UV, winding number, marching cubes,
deformation, detection).

Built on permissive libraries only — **Geogram** (BSD), **Geometry Central** (MIT),
**libigl** core (MPL2) and **Eigen** (MPL2). No CGAL. It deliberately does **not** duplicate
Synera's built-in kernel nor the "Mocha Mesh" marketplace add-in (isotropic remesh and mesh
booleans are intentionally omitted).

## Architecture (three layers)

| Project | Output | Role |
|---|---|---|
| `RaphosGeometryNative` | `RaphosGeometryNative.dll` | C++ `extern "C"` exports over Geogram / Geometry Central / libigl |
| `RaphosGeometryInterop` | `RaphosGeometryInterop.dll` | C# P/Invoke bridge + marshalling (`MeshFunctions`) |
| `RaphosGeometry` | `RaphosGeometry.dll` | Synera `Node` classes (the palette) |
| `TestRaphosGeometry` | — | MSTest headless validation of native + interop |

## Build

Requires Visual Studio 2022+/18 (Desktop C++ with the **v142** toolset) and an installed Synera
(`C:\Program Files\Synera\`). The three geometry libraries must be present and prebuilt at
`C:\dev\{geogram,geometry-central,libigl}`.

```
MSBuild RaphosGeometry.sln -restore -p:Configuration=Release -p:Platform=x64
```

- **Debug / DebugCS** deploy loose files to `C:\ProgramData\Synera\Addins\RaphosGeometry\`.
- **Release** produces a versioned `RaphosGeometry_<version>.synaddin` package.

Tests are validated headlessly with VS's `vstest.console.exe` against the built
`TestRaphosGeometry.dll` (the interop is decoupled from the Synera app runtime for this).

## Status — complete: 38 nodes, all tested (39 MSTest cases green)

All five phases are implemented. Every node has a per-node SVG icon (light + dark) and a
`.syn` example graph, placed exactly like the other add-ins
(`Icons/RaphosGeometryCategory/<Sub>/<Class>.svg`, `Help/RaphosGeometryCategory/<Sub>/<Class>/<Class>.syn`).
The Release `.synaddin` bundles all 38 examples and 79 icons; every example is verified to load and
execute in Synera via `SyneraHeadless.exe info`.

Categories: Remeshing (6) · Point Cloud (11) · Analysis (9) · Parameterization (4) · Deformation (4) · Detection (4).

- **Phase 0 — Skeleton** · **Phase 1 — MVP:** Quadric Decimate, Fill Holes, Heat Geodesic Field,
  Curvature Tensor, Winding Number, Marching Cubes.
- **Phase 2 — imported (permissive libs):** Repair Mesh, Make Consistent, Remove Self-Intersections,
  Exact Geodesic Field, Hausdorff Distance, Geodesic Path (FlipOut), Manifold Harmonics, Vector Heat,
  UV Unwrap (LSCM / Harmonic / ARAP), Auto UV Atlas, Clip Mesh by Plane, ARAP Deformation, Biharmonic
  Weights, Mesh from Point Cloud (Co3Ne), Poisson Reconstruction, Alpha Shape, Remove Outliers,
  Estimate Normals, Orient Normals (MST), Average Spacing, Simplify Point Cloud.
- **Phases 3–4 — from-scratch (CGAL-parity, permissive by construction):** RANSAC Shape Detection,
  Region Growing, Jet Ridges, WLOP Consolidate, Bilateral Denoise, SDF Segmentation, Alpha Wrap,
  Mean-Curvature Skeleton, Advancing Front. (Stitch Borders is covered by Repair Mesh's merge tolerance.)

### Examples

Each node ships a runnable end-to-end `.syn` example under `Help/`. Every example is a real working
graph: a text annotation names the node, **number sliders** drive the scalar parameters (nothing is
hardcoded), and a real **3D dodo model** is loaded via a **relative-path import** — the
`RelativeFilePathContainer` node points at `dodo_small.obj` (a decimated dodo placed next to each
`.syn`, so paths stay relative and portable) feeding `ImportGeometryAsMesh`. Mesh nodes take the
imported mesh directly; point-cloud nodes take its vertices via `DeconstructMesh`. All 38 examples are
verified to load and execute in Synera (`SyneraHeadless.exe`). The dodo asset lives in `_material/dodo`.

### Notes on library gaps found
- Manifold Harmonics: Geogram's spectral solver needs OpenNL's ARPACK extension (absent from the
  prebuilt `geogram.lib`), so it is reimplemented with libigl's cotangent Laplacian + Voronoi mass
  matrix and a dense Eigen generalized eigensolver (fine for small/medium meshes).
- QEM decimation uses libigl `qslim` (Geogram only offers vertex-clustering).
- Poisson uses Geogram's bundled Kazhdan `PoissonRecon` (no first-class wrapper header).
