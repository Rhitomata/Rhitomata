using UnityEngine;

namespace Rhitomata {
    /// <summary>
    /// Custom gizmo drawing functions that works in runtime, should be used on <see cref="Camera.onPostRender"/>
    /// </summary>
    public static class RGizmos {
        private static Material _LineMaterial;

        public static Color Color = new Color(0.2f, 1f, 0.2f, 0.7f);
        private static readonly int srcBlend = Shader.PropertyToID("_SrcBlend");
        private static readonly int dstBlend = Shader.PropertyToID("_DstBlend");
        private static readonly int cull = Shader.PropertyToID("_Cull");
        private static readonly int zWrite = Shader.PropertyToID("_ZWrite");

        /// <summary>
        /// Draws a wireframe representation of a mesh at the specified position, rotation, and scale.
        /// </summary>
        public static void DrawWireMesh(Mesh mesh, Vector3 position, Quaternion rotation, Vector3 scale) {
            if (!mesh) return;

            EnsureLineMaterial();

            _LineMaterial.SetPass(0);

            GL.PushMatrix();

            // Apply transformation
            var transformMatrix = Matrix4x4.TRS(position, rotation, scale);
            GL.MultMatrix(transformMatrix);

            GL.Begin(GL.LINES);
            GL.Color(Color);

            // Draw mesh edges
            DrawMeshEdges(mesh);

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a 3D cube wireframe without diagonal vertex connections, with position, rotation, and scale.
        /// </summary>
        public static void DrawCube(Vector3 position, Quaternion rotation, Vector3 scale) {
            EnsureLineMaterial();
            _LineMaterial.SetPass(0);

            GL.PushMatrix();

            // Apply transformation
            var transformMatrix = Matrix4x4.TRS(position, rotation, scale);
            GL.MultMatrix(transformMatrix);

            GL.Begin(GL.LINES);
            GL.Color(Color);

            // Define the corners of a unit cube
            var corners = new Vector3[]
            {
                new(-0.5f, -0.5f, -0.5f), // 0
                new(0.5f, -0.5f, -0.5f), // 1
                new(0.5f, 0.5f, -0.5f), // 2
                new(-0.5f, 0.5f, -0.5f), // 3
                new(-0.5f, -0.5f, 0.5f), // 4
                new(0.5f, -0.5f, 0.5f), // 5
                new(0.5f, 0.5f, 0.5f), // 6
                new(-0.5f, 0.5f, 0.5f) // 7
            };

            // Define the edges of the cube
            var edges = new[,]
            {
                { 0, 1 }, { 1, 2 }, { 2, 3 }, { 3, 0 }, // Bottom face
                { 4, 5 }, { 5, 6 }, { 6, 7 }, { 7, 4 }, // Top face
                { 0, 4 }, { 1, 5 }, { 2, 6 }, { 3, 7 } // Vertical edges
            };

            // Draw the edges
            for (var i = 0; i < edges.GetLength(0); i++) {
                DrawLine(corners[edges[i, 0]], corners[edges[i, 1]]);
            }

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Draws a sphere wireframe at the specified position, rotation, and scale.
        /// </summary>
        public static void DrawSphere(Vector3 position, Quaternion rotation, float radius,
            int longitudeSegments = 12, int latitudeSegments = 8) {
            EnsureLineMaterial();
            _LineMaterial.SetPass(0);

            GL.PushMatrix();
            var transformMatrix = Matrix4x4.TRS(position, rotation, Vector3.one * radius);
            GL.MultMatrix(transformMatrix);

            GL.Begin(GL.LINES);
            GL.Color(Color);

            // Generate points and draw latitude and longitude lines
            for (var lat = 0; lat <= latitudeSegments; lat++) {
                var theta = Mathf.PI * lat / latitudeSegments; // Angle for latitude
                var sinTheta = Mathf.Sin(theta);
                var cosTheta = Mathf.Cos(theta);

                for (var lon = 0; lon < longitudeSegments; lon++) {
                    var phi = 2 * Mathf.PI * lon / longitudeSegments; // Angle for longitude
                    var nextPhi = 2 * Mathf.PI * (lon + 1) / longitudeSegments;

                    // Current and next points on the same latitude circle
                    var p1 = new Vector3(sinTheta * Mathf.Cos(phi), cosTheta, sinTheta * Mathf.Sin(phi));
                    var p2 = new Vector3(sinTheta * Mathf.Cos(nextPhi), cosTheta, sinTheta * Mathf.Sin(nextPhi));

                    DrawLine(p1, p2); // Draw longitude line
                }
            }

            for (var lon = 0; lon < longitudeSegments; lon++) {
                var phi = 2 * Mathf.PI * lon / longitudeSegments; // Angle for longitude
                var sinPhi = Mathf.Sin(phi);
                var cosPhi = Mathf.Cos(phi);

                for (var lat = 0; lat < latitudeSegments; lat++) {
                    var theta = Mathf.PI * lat / latitudeSegments; // Angle for latitude
                    var nextTheta = Mathf.PI * (lat + 1) / latitudeSegments;

                    // Current and next points on the same longitude circle
                    var p1 = new Vector3(Mathf.Sin(theta) * cosPhi, Mathf.Cos(theta), Mathf.Sin(theta) * sinPhi);
                    var p2 = new Vector3(Mathf.Sin(nextTheta) * cosPhi, Mathf.Cos(nextTheta),
                        Mathf.Sin(nextTheta) * sinPhi);

                    DrawLine(p1, p2); // Draw latitude line
                }
            }

            GL.End();
            GL.PopMatrix();
        }


        /// <summary>
        /// Draws a wireframe frustum using the given parameters.
        /// </summary>
        public static void DrawFrustum(Vector3 position, Quaternion rotation, float fov, float aspect, float near, float far) {
            EnsureLineMaterial();
            _LineMaterial.SetPass(0);

            GL.PushMatrix();
            var transformMatrix = Matrix4x4.TRS(position, rotation, Vector3.one);
            GL.MultMatrix(transformMatrix);

            GL.Begin(GL.LINES);
            GL.Color(Color);

            // Compute the corner points for the near and far planes
            var nearCorners = CalculateFrustumCorners(fov, aspect, near);
            var farCorners = CalculateFrustumCorners(fov, aspect, far);

            // Draw near plane
            for (var i = 0; i < 4; i++) {
                DrawLine(nearCorners[i], nearCorners[(i + 1) % 4]);
            }

            // Draw far plane
            for (var i = 0; i < 4; i++) {
                DrawLine(farCorners[i], farCorners[(i + 1) % 4]);
            }

            // Connect near plane to far plane
            for (var i = 0; i < 4; i++) {
                DrawLine(nearCorners[i], farCorners[i]);
            }

            GL.End();
            GL.PopMatrix();
        }

        /// <summary>
        /// Computes the 4 corner points of a plane at a given distance for a perspective frustum.
        /// </summary>
        private static Vector3[] CalculateFrustumCorners(float fov, float aspect, float distance) {
            var halfHeight = Mathf.Tan(fov * Mathf.Deg2Rad * 0.5f) * distance;
            var halfWidth = halfHeight * aspect;

            return new Vector3[]
            {
                new(-halfWidth,  halfHeight, distance), // Top-left
                new( halfWidth,  halfHeight, distance), // Top-right
                new( halfWidth, -halfHeight, distance), // Bottom-right
                new(-halfWidth, -halfHeight, distance)  // Bottom-left
            };
        }

        /// <summary>
        /// Ensures the line material is created and available for rendering.
        /// </summary>
        private static void EnsureLineMaterial() {
            if (_LineMaterial) return;

            var shader = Shader.Find("Hidden/Internal-Colored");
            _LineMaterial = new Material(shader) {
                hideFlags = HideFlags.HideAndDontSave
            };
            _LineMaterial.SetInt(srcBlend, (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            _LineMaterial.SetInt(dstBlend, (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            _LineMaterial.SetInt(cull, (int)UnityEngine.Rendering.CullMode.Off);
            _LineMaterial.SetInt(zWrite, 0);
        }

        /// <summary>
        /// Draws the edges of a mesh as lines.
        /// </summary>
        private static void DrawMeshEdges(Mesh mesh) {
            var vertices = mesh.vertices;
            var indices = mesh.GetIndices(0);

            for (var i = 0; i < indices.Length; i += 3) {
                var index0 = indices[i];
                var index1 = indices[i + 1];
                var index2 = indices[i + 2];

                DrawLine(vertices[index0], vertices[index1]);
                DrawLine(vertices[index1], vertices[index2]);
                DrawLine(vertices[index2], vertices[index0]);
            }
        }

        /// <summary>
        /// Draws a line between two points in world space.
        /// </summary>
        private static void DrawLine(Vector3 start, Vector3 end) {
            GL.Vertex(start);
            GL.Vertex(end);
        }
    }
}