using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace DualIllusionGenerator
{
    public static class MeshSmoother
    {
        // lambda = inflate step, mu = shrink step (mu is negative and slightly
        // stronger than lambda). This alternating pattern is Taubin smoothing —
        // it removes voxel stair-stepping without the melting/shrinking you get
        // from plain Laplacian smoothing.
        public static void Smooth(MeshData mesh, int iterations, float lambda = 0.5f, float mu = -0.53f)
        {
            if (iterations <= 0) return;

            var neighbors = BuildNeighborMap(mesh);

            for (int iter = 0; iter < iterations; iter++)
            {
                ApplyPass(mesh, neighbors, lambda);
                ApplyPass(mesh, neighbors, mu);
            }
        }

        private static void ApplyPass(MeshData mesh, List<int>[] neighbors, float factor)
        {
            var newPositions = new Vector3[mesh.Vertices.Count];

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var nbrs = neighbors[i];
                if (nbrs.Count == 0) { newPositions[i] = mesh.Vertices[i]; continue; }

                Vector3 avg = Vector3.Zero;
                foreach (int n in nbrs) avg += mesh.Vertices[n];
                avg /= nbrs.Count;

                newPositions[i] = mesh.Vertices[i] + factor * (avg - mesh.Vertices[i]);
            }

            for (int i = 0; i < mesh.Vertices.Count; i++)
                mesh.Vertices[i] = newPositions[i];
        }

        private static List<int>[] BuildNeighborMap(MeshData mesh)
        {
            var sets = new HashSet<int>[mesh.Vertices.Count];
            for (int i = 0; i < sets.Length; i++) sets[i] = new HashSet<int>();

            for (int t = 0; t < mesh.Triangles.Count; t += 3)
            {
                int a = mesh.Triangles[t], b = mesh.Triangles[t + 1], c = mesh.Triangles[t + 2];
                sets[a].Add(b); sets[a].Add(c);
                sets[b].Add(a); sets[b].Add(c);
                sets[c].Add(a); sets[c].Add(b);
            }

            var result = new List<int>[sets.Length];
            for (int i = 0; i < sets.Length; i++) result[i] = new List<int>(sets[i]);
            return result;
        }
    }

    public static class MeshWelder
    {
        public static void Weld(MeshData mesh, float epsilon = 0.0001f)
        {
            var map = new Dictionary<(int, int, int), int>();
            var newVertices = new List<Vector3>();
            var remap = new int[mesh.Vertices.Count];

            (int, int, int) Key(Vector3 v) =>
                ((int)Math.Round(v.X / epsilon),
                 (int)Math.Round(v.Y / epsilon),
                 (int)Math.Round(v.Z / epsilon));

            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                var key = Key(mesh.Vertices[i]);
                if (!map.TryGetValue(key, out int newIndex))
                {
                    newIndex = newVertices.Count;
                    newVertices.Add(mesh.Vertices[i]);
                    map[key] = newIndex;
                }
                remap[i] = newIndex;
            }

            for (int i = 0; i < mesh.Triangles.Count; i++)
                mesh.Triangles[i] = remap[mesh.Triangles[i]];

            mesh.Vertices.Clear();
            mesh.Vertices.AddRange(newVertices);
        }
    }
}
