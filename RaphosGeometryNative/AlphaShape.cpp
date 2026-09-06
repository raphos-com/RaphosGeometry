#include "AlphaShape.h"
#include <geogram/basic/common.h>
#include <geogram/basic/command_line.h>
#include <geogram/basic/command_line_args.h>
#include <geogram/delaunay/delaunay.h>
#include <vector>
#include <array>
#include <cmath>
#include <algorithm>
#include <set>
#include <unordered_map>
#include <memory>
#include <unordered_set>
#include <numeric>
#include <vector>
#include <array>
#include <cmath>
#include <algorithm>
#include <set>
#include <unordered_map>
#include <limits>
#include <queue>

template<typename T = double>
struct Point3D {
    T x, y, z;

    Point3D(T x = 0, T y = 0, T z = 0) : x(x), y(y), z(z) {}

    T distanceTo(const Point3D& other) const {
        T dx = x - other.x;
        T dy = y - other.y;
        T dz = z - other.z;
        return std::sqrt(dx * dx + dy * dy + dz * dz);
    }

    Point3D operator-(const Point3D& other) const {
        return Point3D(x - other.x, y - other.y, z - other.z);
    }

    Point3D operator+(const Point3D& other) const {
        return Point3D(x + other.x, y + other.y, z + other.z);
    }

    Point3D operator*(const T& v) const {
        return Point3D(x * v, y * v, z * v);
    }

    Point3D operator/(const T& v) const {
        return Point3D(x / v, y / v, z / v);
    }

    T dot(const Point3D& other) const {
        return x * other.x + y * other.y + z * other.z;
    }

    Point3D cross(const Point3D& other) const {
        return Point3D(
            y * other.z - z * other.y,
            z * other.x - x * other.z,
            x * other.y - y * other.x
        );
    }

    T lengthSquared() const {
        return x * x + y * y + z * z;
    }
};

template<typename T = double>
struct Triangle {
    std::array<size_t, 3> vertices;
    const std::vector<Point3D<T>>* pointsRef;

    Triangle(size_t v1, size_t v2, size_t v3, const std::vector<Point3D<T>>* points)
        : pointsRef(points) {
        vertices = { v1, v2, v3 };
        std::sort(vertices.begin(), vertices.end());
    }

    bool operator==(const Triangle& other) const {
        return vertices == other.vertices;
    }

    bool operator<(const Triangle& other) const {
        return vertices < other.vertices;
    }
};

template<typename T>
struct TriangleHash {
    size_t operator()(const Triangle<T>& triangle) const {
        size_t hash = 17;
        for (const auto& vertex : triangle.vertices) {
            hash = hash * 31 + std::hash<size_t>{}(vertex);
        }
        return hash;
    }
};

template<typename T = double>
class AlphaShape3D {
private:
    struct Tetrahedron {
        std::array<size_t, 4> vertices;
        T circumRadius;
        Point3D<T> circumCenter;
        bool isValid;

        Tetrahedron(size_t v1, size_t v2, size_t v3, size_t v4)
            : vertices({ v1, v2, v3, v4 }), circumRadius(0), isValid(true) {
            std::sort(vertices.begin(), vertices.end());
        }

        bool operator==(const Tetrahedron& other) const {
            return vertices == other.vertices;
        }
    };

    const T EPSILON;
    std::vector<Point3D<T>> points;
    T alpha;
    std::vector<Triangle<T>> faces;
    std::vector<Tetrahedron> tetrahedra;

    bool isDegenerate(const Point3D<T>& p1, const Point3D<T>& p2,
        const Point3D<T>& p3, const Point3D<T>& p4) const {
        Point3D<T> v1 = p2 - p1;
        Point3D<T> v2 = p3 - p1;
        Point3D<T> v3 = p4 - p1;

        T volume = std::abs(v1.cross(v2).dot(v3));
        return volume < EPSILON;
    }

    bool computeCircumsphere(Tetrahedron& tetra) {
        const Point3D<T>& p0 = points[tetra.vertices[0]];
        const Point3D<T>& p1 = points[tetra.vertices[1]];
        const Point3D<T>& p2 = points[tetra.vertices[2]];
        const Point3D<T>& p3 = points[tetra.vertices[3]];

        if (isDegenerate(p0, p1, p2, p3)) {
            tetra.isValid = false;
            return false;
        }

        // Matrix solution for circumcenter computation
        Point3D<T> v10 = p1 - p0;
        Point3D<T> v20 = p2 - p0;
        Point3D<T> v30 = p3 - p0;

        T d10 = v10.lengthSquared();
        T d20 = v20.lengthSquared();
        T d30 = v30.lengthSquared();

        T denom = 2 * (v10.cross(v20).dot(v30));
        if (std::abs(denom) < EPSILON) {
            tetra.isValid = false;
            return false;
        }

        Point3D<T> c20 = v20.cross(v30);
        Point3D<T> c30 = v30.cross(v10);
        Point3D<T> c10 = v10.cross(v20);

        T alpha = d30 * c10.dot(c10) / denom;
        T beta = d10 * c30.dot(c30) / denom;
        T gamma = d20 * c20.dot(c20) / denom;

        tetra.circumCenter = Point3D<T>(
            p0.x + alpha * v10.x + beta * v20.x + gamma * v30.x,
            p0.y + alpha * v10.y + beta * v20.y + gamma * v30.y,
            p0.z + alpha * v10.z + beta * v20.z + gamma * v30.z
        );

        tetra.circumRadius = tetra.circumCenter.distanceTo(p0);

        auto circumCenter = p0 + (c20 * d10 + c30 * d20 + c10 * d30) / (2 * v10.dot(c20));
        tetra.circumCenter = circumCenter;
        tetra.circumRadius = tetra.circumCenter.distanceTo(p0);

        return true;
    }

    void computeDelaunayTetrahedralization() {
        if (points.size() < 4) return;

        // Delaunay tetrahedralization via Geogram (near-linear, exact predicates) instead of a naive
        // O(n^2) Bowyer-Watson insertion. Point3D<double> is {x,y,z} contiguous, so the points vector
        // is already an xyzxyz double array Geogram can consume directly. Cells are finite tetrahedra
        // indexing into `points`; the alpha filter + boundary-face extraction below are unchanged.
        GEO::initialize();
        GEO::CmdLine::import_arg_group("algo");
        GEO::Delaunay_var delaunay = GEO::Delaunay::create(3, "BDEL");
        delaunay->set_vertices((GEO::index_t)points.size(), &points[0].x);

        tetrahedra.clear();
        tetrahedra.reserve(delaunay->nb_cells());
        for (GEO::index_t c = 0; c < delaunay->nb_cells(); ++c) {
            GEO::signed_index_t a = delaunay->cell_vertex(c, 0);
            GEO::signed_index_t b = delaunay->cell_vertex(c, 1);
            GEO::signed_index_t d = delaunay->cell_vertex(c, 2);
            GEO::signed_index_t e = delaunay->cell_vertex(c, 3);
            if (a < 0 || b < 0 || d < 0 || e < 0) continue;   // skip any infinite cell
            Tetrahedron tetra((size_t)a, (size_t)b, (size_t)d, (size_t)e);
            if (computeCircumsphere(tetra)) tetrahedra.push_back(tetra);
        }
    }

    void extractAlphaShape() {
        std::set<Triangle<T>> alpha_faces;
        std::unordered_map<Triangle<T>, int, TriangleHash<T>> face_count;

        // Count face occurrences
        for (const auto& tetra : tetrahedra) {
            if (!tetra.isValid || tetra.circumRadius > alpha) continue;

            for (int i = 0; i < 4; ++i) {
                size_t v1 = tetra.vertices[i];
                size_t v2 = tetra.vertices[(i + 1) % 4];
                size_t v3 = tetra.vertices[(i + 2) % 4];
                Triangle<T> tri(v1, v2, v3, &points);
                face_count[tri]++;
            }
        }

        // Add faces that appear exactly once (boundary faces)
        for (const auto& pair : face_count) {
            if (pair.second == 1) {
                alpha_faces.insert(pair.first);
            }
        }

        faces.assign(alpha_faces.begin(), alpha_faces.end());
    }

public:
    AlphaShape3D(const std::vector<Point3D<T>>& input_points, T alpha_value, T epsilon = T(1e-10))
        : points(input_points), alpha(alpha_value), EPSILON(epsilon) {
        if (!points.empty()) {
            computeDelaunayTetrahedralization();
            extractAlphaShape();
        }
    }

    const std::vector<Triangle<T>>& getFaces() const {
        return faces;
    }

    const std::vector<Point3D<T>>& getPoints() const {
        return points;
    }
};


int AlphaShape(
    double* pnts,
    Long pntCount,
    Long** facesPntIndices,
    Long& facesCount,
    double alpha
) {
    std::vector<Point3D<>> points;

    for (size_t i = 0; i < pntCount; i++)
    {
        points.push_back(Point3D<>(
            pnts[i * 3 + 0],
            pnts[i * 3 + 1],
            pnts[i * 3 + 2]
        ));
    }
    const double epsilon = 1e-3;  // Numerical tolerance

    AlphaShape3D<> alpha_shape(points, alpha, epsilon);

    const std::vector<Triangle<>>& faces = alpha_shape.getFaces();

    facesCount = faces.size();
    Long* faceCountPtr = new Long[facesCount * 3];

    for (size_t i = 0; i < faces.size(); i++)
    {
        faceCountPtr[i * 3 + 0] = (Long)faces[i].vertices[0];
        faceCountPtr[i * 3 + 1] = (Long)faces[i].vertices[1];
        faceCountPtr[i * 3 + 2] = (Long)faces[i].vertices[2];
    }
    *facesPntIndices = faceCountPtr;

    return RAPHOS_SUCCESS;
}