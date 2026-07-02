using System;
using System.Collections.Generic;
using System.Text;

namespace DualIllusionGenerator
{
    public static class VoxelFaceBuilder
    {
        public struct FaceMeshData
        {
            public List<System.Numerics.Vector3> Vertices;
            public List<int> Triangles;
            public List<System.Numerics.Vector3> Normals; // per-vertex, matches Vertices
        }

        public static FaceMeshData Build(VoxelGrid grid)
        {
            var verts = new List<System.Numerics.Vector3>();
            var tris = new List<int>();
            var normals = new List<System.Numerics.Vector3>();
            object writeLock = new object();
            float s = grid.VoxelSize;

            Parallel.For(0, grid.Width, () => (new List<System.Numerics.Vector3>(), new List<int>(), new List<System.Numerics.Vector3>()),
                (x, _, local) =>
                {
                    var (lv, lt, ln) = local;
                    float x0 = x * s, x1 = (x + 1) * s;

                    for (int y = 0; y < grid.Height; y++)
                    {
                        float y0 = y * s, y1 = (y + 1) * s;
                        for (int z = 0; z < grid.Depth; z++)
                        {
                            if (!grid.GetVoxel(x, y, z)) continue;
                            float z0 = z * s, z1 = (z + 1) * s;

                            void Quad(System.Numerics.Vector3 n, System.Numerics.Vector3 a, System.Numerics.Vector3 b, System.Numerics.Vector3 c, System.Numerics.Vector3 d)
                            {
                                int idx = lv.Count;
                                lv.Add(a); lv.Add(b); lv.Add(c); lv.Add(d);
                                ln.Add(n); ln.Add(n); ln.Add(n); ln.Add(n);
                                lt.Add(idx); lt.Add(idx + 1); lt.Add(idx + 2);
                                lt.Add(idx); lt.Add(idx + 2); lt.Add(idx + 3);
                            }

                            if (!grid.GetVoxel(x - 1, y, z)) Quad(new(-1, 0, 0), new(x0, y0, z0), new(x0, y0, z1), new(x0, y1, z1), new(x0, y1, z0));
                            if (!grid.GetVoxel(x + 1, y, z)) Quad(new(1, 0, 0), new(x1, y0, z0), new(x1, y1, z0), new(x1, y1, z1), new(x1, y0, z1));
                            if (!grid.GetVoxel(x, y - 1, z)) Quad(new(0, -1, 0), new(x0, y0, z0), new(x1, y0, z0), new(x1, y0, z1), new(x0, y0, z1));
                            if (!grid.GetVoxel(x, y + 1, z)) Quad(new(0, 1, 0), new(x0, y1, z0), new(x0, y1, z1), new(x1, y1, z1), new(x1, y1, z0));
                            if (!grid.GetVoxel(x, y, z - 1)) Quad(new(0, 0, -1), new(x0, y0, z0), new(x0, y1, z0), new(x1, y1, z0), new(x1, y0, z0));
                            if (!grid.GetVoxel(x, y, z + 1)) Quad(new(0, 0, 1), new(x0, y0, z1), new(x1, y0, z1), new(x1, y1, z1), new(x0, y1, z1));
                        }
                    }
                    return (lv, lt, ln);
                },
                local =>
                {
                    lock (writeLock)
                    {
                        int offset = verts.Count;
                        verts.AddRange(local.Item1);
                        normals.AddRange(local.Item3);
                        foreach (var t in local.Item2) tris.Add(t + offset);
                    }
                });

            return new FaceMeshData { Vertices = verts, Triangles = tris, Normals = normals };
        }
    }
}
