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
- **Release** obfuscates both managed assemblies with Obfuscar (`KeepPublicApi` — Synera
  still finds node types by their `[Guid]`), then produces a versioned
  `RaphosGeometry_<version>.synaddin` package.

### Required Geogram patch (Debug builds under newer MSVC)
Geogram's `Memory::aligned_allocator` (`src/lib/geogram/basic/memory.h`) predates the C++
Allocator requirement that an allocator be constructible from a rebound allocator of a
different value type. Newer MSVC standard libraries exercise exactly that under
`_ITERATOR_DEBUG_LEVEL != 0` — i.e. **Debug** builds — where a container rebinds its
allocator to `std::_Container_proxy` and copies it from a `const` allocator, producing:

```
xmemory(...): error C2440: 'static_cast': cannot convert from 'const _Alloc'
              to 'GEO::Memory::aligned_allocator<U,64>'   [U = std::_Container_proxy]
```

Fix (applied in the local `C:\dev\geogram` checkout — it lives outside this repo, so it is
recorded here rather than committed): add a templated converting constructor to
`aligned_allocator` (and an explicit default constructor, since declaring the converting
one suppresses the implicit default), and make the existing conversion operator `const`:

```cpp
aligned_allocator() = default;
template <class U, int A2>
aligned_allocator(const aligned_allocator<U, A2>&) noexcept { }
// ...and further down:
template <class T2, int A2> operator aligned_allocator<T2, A2>() const { ... }
```

The change is additive and stateless (Release builds compile without it, since
`_ITERATOR_DEBUG_LEVEL == 0` never instantiates the proxy path).

Tests are validated headlessly with VS's `vstest.console.exe` against the built
`TestRaphosGeometry.dll` (the interop is decoupled from the Synera app runtime for this).

## Status — complete: 39 nodes, all tested

All five phases are implemented. Every node has a per-node SVG icon (light + dark) and a
`.syn` example graph, placed exactly like the other add-ins
(`Icons/RaphosGeometryCategory/<Sub>/<Class>.svg`, `Help/RaphosGeometryCategory/<Sub>/<Class>/<Class>.syn`).
The Release `.synaddin` bundles every example and icon. Each example is verified to load **and
actually execute** in Synera via `SyneraHeadless.exe execute`, and — because "finished successfully"
hides per-node errors — every example is additionally checked two ways: its per-node error/warning
table is inspected, and its main output is **saved to a file and analysed** (vertex/point counts,
scalar/vector ranges) to confirm the result is non-empty and meaningful, not just that the graph ran.

The palette category **Raphos Geometry** has three subcategories, kept few so the ribbon stays compact
(each subcategory is one ribbon group):
- **Mesh** (15) — remeshing/repair, UV parameterization, deformation, marching cubes (grid + field).
- **Analysis** (9) — curvature, geodesics, winding number, spectral/heat fields, distances.
- **Point Cloud** (15) — reconstruction, normals, denoise/simplify, and shape/feature detection.

- **Phase 0 — Skeleton** · **Phase 1 — MVP:** Quadric Decimate, Fill Holes, Heat Geodesic Field,
  Curvature Tensor, Winding Number, Marching Cubes, Marching Cubes (Field).
- **Phase 2 — imported (permissive libs):** Repair Mesh, Make Consistent, Remove Self-Intersections,
  Exact Geodesic Field, Hausdorff Distance, Geodesic Path (FlipOut), Manifold Harmonics, Vector Heat,
  UV Unwrap (LSCM / Harmonic / ARAP), Auto UV Atlas, Clip Mesh by Plane, ARAP Deformation, Biharmonic
  Weights, Mesh from Point Cloud (Co3Ne), Poisson Reconstruction, Alpha Shape, Remove Outliers,
  Estimate Normals, Orient Normals (MST), Average Spacing, Simplify Point Cloud.
- **Phases 3–4 — from-scratch (CGAL-parity, permissive by construction):** RANSAC Shape Detection,
  Region Growing, Jet Ridges, WLOP Consolidate, Bilateral Denoise, SDF Segmentation, Alpha Wrap,
  Mean-Curvature Skeleton, Advancing Front. (Stitch Borders is covered by Repair Mesh's merge tolerance.)

### Examples

Each node ships a runnable end-to-end `.syn` example under `Help/`, built to be read by a novice: it
shows the node **working** on the dodo and **visualizes its output**, so you can see what the node does
rather than just wire it up. Every example has two annotations — the node name (large) and a
**description paragraph** (default size, from the node's own description); the geometry is loaded via a
**relative-path import** (`RelativeFilePathContainer` → `ImportStl`, the mesh file placed next to each
`.syn`); every **scalar parameter is a number slider** (tunable, not hardcoded); and point/plane/grid
constants are internalized on the node inputs.

The output is visualized per result type:
- **result mesh / point cloud** (decimate, fill holes, repair, remesh, clip, marching cubes, ARAP
  deform, reconstruction, denoise/simplify): previewed directly.
- **per-vertex scalar field** (geodesic distance, curvature, harmonics, SDF, biharmonic weights,
  winding number): colors the mesh through `Bounding Interval → Remap → Construct/Evaluate Color Map
  → Mesh Colors`.
- **direction / normal field** (curvature principal directions, estimated/oriented normals): drawn as
  line segments with `Line SDL` — for curvature, the length is the principal curvature, so the lines
  are proportional to it.
- **UV unwrap** (LSCM / Harmonic / ARAP): shown as a comprehensive *why-UV-matters* demo. A geodesic
  field is painted on the 3D surface, and the **same field** is shown on both the 3D mesh and its
  flattened unwrap (`Construct Mesh` from the per-vertex UVs + the original faces, coloured by the
  field). Identical patterns on both = the bijective 3D↔2D map that lets you paint/bake a texture in
  2D and have it wrap onto the model. The flat map sits at z=0 below the 3D patch, so both are visible.
- **Auto UV Atlas** emits one UV per face-corner (three per triangle, not per vertex). A raw UV list is
  hard to read, so the node maps the UVs **back onto the model**: **Textured Mesh** is the 3D dodo
  with a checkerboard painted through its UVs (even squares = low distortion) and **Atlas Mesh** is the
  packed 2D layout you would bake into. `Checker Squares` tunes the texture frequency.
- **Fill Holes** also returns each hole's patch as its own mesh (**Patches**) plus the number of holes
  filled (**Filled**): the example previews the patches to highlight exactly what was added, and shows
  the count in a panel.
- **ARAP Deformation** is interactive: one handle is pinned to the dodo's head and its target position
  is built from three **Target X/Y/Z** sliders, so you can drag the head and watch the body follow.
- **Repair Mesh** / **Make Consistent** direct the eye to what actually changed. Repair Mesh runs on
  `messy.stl` and shows the triangle count **before and after** in two panels (bad faces removed).
  Make Consistent runs on `dodo_flipped.stl` and draws the per-vertex normals as arrows **before**
  (stabbing inward in the flipped patches) and **after** (all pointing cleanly outward).

Geometry is a real **3D dodo model** (from `raphos-website/artifacts/dodo`, in `_material/dodo`).
`ImportGeometryAsMesh` is Parasolid-based and does **not** read `.obj`, so the dodo is provided as
**STL** (Synera's native triangle-mesh format, read by `ImportStl`).

The original `dodo.stl` was a decimated **multi-part scan — 46 disconnected shells** — which looked
broken and misled any solver that needs a single manifold. It has been **retired from every example**:
all mesh and point-cloud examples now use the clean genus-0 **`dodo_clean.stl`** (its vertices make a
good, uniform point cloud too), so the old dodo no longer appears anywhere. The demo meshes (in
`_material/dodo`):
- **`dodo_clean.stl`** — a **genus-0 watertight dodo**, still recognisably the dodo. Made from
  `dodo_full.stl` by this add-in's own **Alpha Wrap** node (signed-distance field on a 160³ grid →
  isosurface, i.e. a marching-cubes shrink-wrap) then **Quadric Decimate** to ~6k triangles. Used by
  essentially every mesh and point-cloud example (deformation, weights, geodesics, spectral, curvature,
  reconstruction, normals, denoise/simplify, decimation, segmentation, atlas, …).
- **`dodo_disk.stl`** — the clean dodo clipped by a horizontal plane (legs removed, via this add-in's
  own Clip Mesh by Plane node) → an on-brand open disk (one boundary loop) for the UV unwraps.
- **`torus.stl`** — a clean genus-1 tube → mean-curvature skeleton (it contracts to the centre circle).
- **`slice.stl`** — a vertical plane through the standing dodo → winding number samples it, colouring
  each point inside (≈1) / outside (≈0) so you see the dodo's cross-section profile.
- **`planes.stl`** — a subdivided cube whose six planes give RANSAC / Region Growing several segments.
- **`blob.stl`** — a small clean convex-ish cloud → Alpha Shape's point set.
- **`dodo_full.stl`** — the full-res dodo, the second mesh in the Hausdorff example (nominal vs decimated).

Handle placement matters: ARAP snaps handles to the nearest surface vertex, but biharmonic weights'
`igl::boundary_conditions` only registers a handle within ~bbox·1e-3 of an actual vertex, so its two
handles are exact `dodo_clean` vertices (head and feet).

All 38 examples are verified to execute cleanly in Synera (`SyneraHeadless.exe execute`).

### Notes on library gaps found
- Manifold Harmonics: Geogram's spectral solver needs OpenNL's ARPACK extension (absent from the
  prebuilt `geogram.lib`), so it is reimplemented with libigl's cotangent Laplacian + Voronoi mass
  matrix. It solves the k smallest eigenpairs with **geometry-central's sparse inverse-power-iteration
  solver** (`smallestKEigenvectorsPositiveDefinite`, ~O(k·nnz) with one sparse factorization) rather
  than a dense Eigen eigensolver — ~0.3 s vs ~76 s on the 3k-vertex dodo, so it runs on full-res meshes.
- QEM decimation uses libigl `qslim` (Geogram only offers vertex-clustering).
- Poisson uses Geogram's bundled Kazhdan `PoissonRecon` (no first-class wrapper header).
- Alpha Shape tetrahedralizes with Geogram's `Delaunay` (exact predicates, near-linear) and keeps the
  boundary faces of the tetrahedra whose circumradius is below alpha — instant even on the full cloud.

### Repair / cleanup examples run on a defective input (so the effect is real, not a no-op)
These nodes do nothing on an already-clean mesh, so each is fed the specific defect it fixes, and all
were verified to actually change the geometry:
- **Fill Holes** → `dodo_holed.stl` (dodo with 12 small holes) → **watertight** output (12 boundary
  loops → 0). The native node is fine; an earlier no-op was a generator bug (the "Max Hole Edges = 0
  means no limit" input was being clamped to 1, so only ≤1-edge holes were considered).
- **Remove Self-Intersections** → `selfx.stl` (two interpenetrating boxes) → resolved (24 → 72 tris).
- **Repair Mesh** → `messy.stl` (clean dodo + 30 duplicate + 20 degenerate faces) → cleaned. The
  example shows the triangle count before and after in two panels. (Bug fixed: with *Triangulate* on,
  the native call asserted `variable_exists` inside Geogram — its re-triangulation reads algorithm
  CmdLine variables whose arg groups were never imported — so the node threw "External component has
  thrown an exception" and produced nothing. Now imports `standard`+`algo` arg groups, like Fill Holes.)
- **Remove Outliers** → the clean dodo cloud (642 pts) + 30 scattered flyers → flyers removed. The
  N-th-neighbour test is sensitive to density: `Neighbours` is set to **8** (70 on this sparse a cloud
  would flag every point and return nothing).
- **Make Consistent** → `dodo_flipped.stl` (clean dodo with ~40% of faces reversed) → coherently
  re-oriented (verified: a valid closed manifold out).
- **Bilateral Denoise** → a noisy blob cloud (points jittered along their normals) + the true normals
  → points pulled back onto the smooth surface.
- **WLOP Consolidate** → the same noisy cloud → denoised and evenly redistributed.
- **Orient Normals** → a clean blob cloud + **random-signed** normals → all flipped to point
  consistently outward (drawn as arrows at the cloud points).
