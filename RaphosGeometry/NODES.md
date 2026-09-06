# Raphos Geometry — node reference

What each node does, and what it's good for. Both columns now show on each example's canvas: the
**Description** paragraph followed by a **Real-world use** line drawn from the **Useful for** column
below. This table stays as the at-a-glance reference for all nodes.

## Mesh

| Node | Description | Useful for (real-world example) |
|---|---|---|
| **Quadric Decimate** | Reduce a triangle mesh to a target face count using QEM edge-collapse decimation (QSlim). | Making a heavy scan or CAD tessellation light enough for real-time viewing, web/VR, or a level-of-detail chain — e.g. turning a 5 M-triangle scanned engine block into a 50 k-triangle preview that still looks the same. |
| **Fill Holes** | Fill boundary holes in a triangle mesh. A maximum hole area of 0 fills every hole. | Closing the gaps a scanner leaves (the un-seen underside of an object) so the mesh becomes watertight for 3D printing or simulation. |
| **Repair Mesh** | Clean up a triangle mesh: merge colocated vertices, remove duplicate and degenerate facets, and optionally re-triangulate. | First-pass cleanup of a downloaded or scanned STL before anything else — removing the duplicate vertices and sliver triangles that break booleans, slicing, and solvers. |
| **Make Consistent** | Coherently reorient the facets of a triangle mesh so neighbouring triangles wind the same way. | Fixing the "flipped normals / black patches" you get from a bad CAD or STL export, so shading, printing, and inside/outside tests behave. |
| **Remove Self-Intersections** | Resolve self-intersections in a triangle mesh into a clean, intersection-free triangulation using exact arithmetic. | Cleaning geometry that overlaps itself after offsetting or a boolean, so it's valid for 3D printing or FEA meshing. |
| **Clip Mesh by Plane** | Clip a triangle mesh with a plane, keeping the half on the back side of the plane normal. Straddling triangles are split cleanly along the plane. | Making a section / cut-away view of a part, or trimming geometry to a region of interest (e.g. cutting a terrain scan down to the build plot). |
| **Alpha Wrap** | Produce a watertight shrink-wrap of a messy or open mesh by sampling a signed-distance field on a grid and extracting the offset isosurface. | Sealing a noisy or incomplete scan (a mechanical part full of holes and self-intersections) into a single watertight solid ready for CFD meshing or printing. |
| **Marching Cubes** | Extract an isosurface triangle mesh from a scalar field sampled on a regular grid (you supply the grid points and values). | Turning volumetric data into a surface — an organ isosurface from a CT/MRI scan, or an implicit/metaball shape into a printable mesh. |
| **Marching Cubes (Field)** | Extract an isosurface from a field over a box: give the domain corners, the resolution and one value per sample, and the node builds the grid for you. | Meshing an implicit shape, an SDF or simulation field directly from values, without hand-building the sample grid. |
| **ARAP Deformation** | As-rigid-as-possible handle-based deformation: the nearest vertex to each handle point is constrained to its target and the mesh follows as rigidly as possible. | Posing or editing an organic model by dragging a few handles — bending a scanned arm or tweaking a product shape — without crushing the local detail. |
| **Biharmonic Weights** | Bounded biharmonic skinning weights for a set of point handles: smooth, non-negative and partition-of-unity. Outputs the influence of one handle as a per-vertex field. | Binding a mesh to control handles or bones for smooth deformation — character rigging, or spreading an influence/load smoothly across a surface. |
| **UV Unwrap (LSCM)** | Least-squares conformal UV unwrapping of an open (disk-topology) mesh. One UV coordinate per vertex in the XY plane. | An angle-preserving flatten for texturing where local detail must not shear — e.g. mapping a brand logo cleanly onto a curved shoe last. |
| **UV Unwrap (ARAP)** | As-rigid-as-possible UV unwrapping (free boundary), harmonic-initialized and ARAP-refined for low angular/area distortion. Requires an open mesh. | A low-distortion unwrap where proportions matter — applying a printed graphic to a curved panel so it isn't stretched. |
| **Harmonic Parameterization** | Fixed-boundary harmonic UV parameterization: the boundary is pinned to a circle and the interior solved from the Laplace equation. Requires an open (disk) mesh. | A quick, guaranteed-foldover-free flatten of a surface patch for texture mapping or for doing 2D analysis on a curved panel. |
| **Auto UV Atlas** | Segment a mesh into charts along sharp edges and flatten + pack them. One UV per face-corner so seams are preserved. | Auto-generating a full UV layout so a texture or decal can be baked onto an arbitrary model (a game asset, a product render) with no manual seam work. |

## Analysis

| Node | Description | Useful for (real-world example) |
|---|---|---|
| **Curvature Tensor** | Per-vertex principal curvature tensor via robust quadric fitting: principal directions, curvatures k1/k2, and Gaussian/mean curvature. | Finding where a surface bends most — spotting stress-concentrating fillets on a bracket, or reading the character lines of a car body for styling/QA. |
| **Heat Geodesic Field** | Geodesic distance from source points to every vertex (heat method). One value per vertex. | A fast "distance-along-the-surface" heatmap — distance from an inlet or a weld, or a sizing field for remeshing — when exact geodesics are too slow. |
| **Exact Geodesic Field** | Exact polyhedral geodesic distance (MMP) from source points to every vertex. | The precise on-surface distance — true cable/seam length along a curved hull, or a walking distance across terrain; the ground truth the heat method approximates. |
| **Geodesic Path** | Shortest geodesic path along a surface between two points (FlipOut edge-flip). | The actual shortest route over a surface — routing a cable on a fuselage, placing a seam on upholstery, or a trail across terrain. |
| **Vector Heat** | Parallel-transport a direction from a source point across the whole surface. One vector per vertex. | Smoothly spreading a direction over a surface — defining a consistent fibre/grain/brush direction for a composite layup or a texture from a single seed. |
| **Winding Number** | Generalized winding number of query points w.r.t. a mesh: ~1 inside, ~0 outside, robust on imperfect meshes. | A robust inside/outside test on messy geometry — voxelizing or point-classifying against a scan that isn't perfectly watertight. |
| **Manifold Harmonics** | Laplace-Beltrami eigenfunctions (the spectral / "manifold Fourier" basis): eigenvalues and one selected eigenfunction. | The "Fourier transform" of a surface — spectral smoothing, shape signatures for matching/retrieval, or low-frequency deformation. |
| **Mean-Curvature Skeleton** | Contract a mesh toward its curve skeleton via mean-curvature (Laplacian) flow. Tubular parts collapse onto their centrelines. | Extracting a 1-D centreline — the axis of a blood vessel or a pipe network for measurement, graph analysis, or animation rigging. |
| **Hausdorff Distance** | Directed A→B, B→A and symmetric Hausdorff distance between two meshes — the worst-case gap. | Tolerance QA — does a manufactured or scanned part stay within spec of the nominal CAD? Reports the single largest deviation, not just the average. |

## Point Cloud

| Node | Description | Useful for (real-world example) |
|---|---|---|
| **Estimate Normals** | Unit normal at each point by PCA of its k nearest neighbours. Orientation is not resolved here. | The first step in turning a raw scan into a surface — normals are required by Poisson reconstruction, bilateral denoise, and lighting. |
| **Orient Normals** | Consistently orient normals along a minimum spanning tree of the k-NN graph (Hoppe et al.). | Making all normals point outward so Poisson reconstruction and shading don't flip — fixing the "some normals inverted" scan. |
| **Average Spacing** | Mean nearest-neighbour spacing of a point cloud — a sizing field. | Auto-picking radii for the other point-cloud tools so reconstruction/simplification parameters aren't guessed from the size of the object. |
| **Remove Outliers** | Remove points whose N-th nearest neighbour is farther than a radius. | Deleting scanner "flyers" and dust from a photogrammetry or LiDAR cloud before meshing. |
| **Simplify Point Cloud** | Voxel-grid downsample: one centroid per occupied cell of the given size. | Uniformly thinning a huge LiDAR scan to a manageable size while keeping full coverage. |
| **Bilateral Denoise** | Feature-preserving denoise: move each point along its normal by a bilateral (spatial + normal) average. Needs normals. | Cleaning scanner noise from a cloud while keeping edges and creases sharp, instead of rounding them off. |
| **WLOP Consolidate** | Weighted Locally Optimal Projection: denoise and evenly redistribute a cloud (attraction + repulsion). | Turning a noisy, unevenly-sampled scan into a clean, uniform cloud that meshes well. |
| **Mesh from Point Cloud** | Reconstruct a triangle mesh from points (Geogram Co3Ne). Normals improve the result. | A fast triangulation of a scan when a full watertight Poisson surface is overkill. |
| **Poisson Reconstruction** | Screened Poisson surface reconstruction from an oriented point cloud (needs normals); watertight and noise-tolerant. | The go-to for turning photogrammetry/LiDAR of an object into a closed, printable surface. |
| **Advancing Front** | Ball-pivoting reconstruction: a ball of the given radius rolls over the samples emitting triangles. Radius 0 auto-picks. | Surfacing a clean, dense scan into a mesh that faithfully follows the sample points — reverse-engineering a scanned object. |
| **Alpha Shape** | Boundary of the Delaunay tetrahedra whose circumradius is below alpha; smaller alpha carves more concavity. | Extracting the concave footprint/shape of a point set — the outline of a scanned room, or the coverage area of scattered survey points. |
| **RANSAC Shape Detection** | Detect multiple primitives (planes, spheres, cylinders) in a cloud. Primitive index per point (-1 = unassigned). | Reverse-engineering a mechanical scan into CAD primitives, or segmenting a building facade / piping run into planes and cylinders. |
| **Region Growing** | Segment a cloud into smooth (near-planar) regions by growing from seeds while normals stay consistent. | Splitting a scanned room into walls / floor / ceiling, or a part into its faces, for downstream fitting or measurement. |
| **Jet Ridges** | Per-point principal curvatures via Cazals-Pouget jet fitting; k1/k2 and a ridge strength that highlights feature lines. | Detecting sharp edges / crease lines in a scan — locating the machined edges of a part for feature-aware processing. |
| **SDF Segmentation** | Segment a mesh by Shape Diameter Function (local thickness) — a semantic part split, not a dihedral-angle split. | Separating a model into meaningful parts by thickness — a character's limbs from its torso, or a part's thin webs from its thick bosses. |
