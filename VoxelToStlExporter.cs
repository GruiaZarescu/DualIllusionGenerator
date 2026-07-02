using System;
using System.Collections.Generic;
using System.Text;

namespace DualIllusionGenerator
{
    using System.IO;
    using System.Numerics;
    using System.Threading.Tasks;

    public static class VoxelToStlExporter
    {
        public static void Export(VoxelGrid grid, string filePath)
        {
            // 1. Count total triangles in parallel first (needed for STL header)
            int totalTriangles = 0;
            object countLock = new object();

            Parallel.For(0, grid.Width, x =>
            {
                int localTriangles = 0;
                for (int y = 0; y < grid.Height; y++)
                {
                    for (int z = 0; z < grid.Depth; z++)
                    {
                        if (!grid.GetVoxel(x, y, z)) continue;

                        if (!grid.GetVoxel(x - 1, y, z)) localTriangles += 2;
                        if (!grid.GetVoxel(x + 1, y, z)) localTriangles += 2;
                        if (!grid.GetVoxel(x, y - 1, z)) localTriangles += 2;
                        if (!grid.GetVoxel(x, y + 1, z)) localTriangles += 2;
                        if (!grid.GetVoxel(x, y, z - 1)) localTriangles += 2;
                        if (!grid.GetVoxel(x, y, z + 1)) localTriangles += 2;
                    }
                }
                // Safely add to the total
                System.Threading.Interlocked.Add(ref totalTriangles, localTriangles);
            });

            // 2. Write the file
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                writer.Write(new byte[80]); // Header
                writer.Write(totalTriangles); // Triangle count

                object fileLock = new object();
                float s = grid.VoxelSize;

                // 3. Generate and write triangles in parallel
                Parallel.For(0, grid.Width, x =>
                {
                    float x0 = x * s;
                    float x1 = (x + 1) * s;

                    // Each thread gets its own MemoryStream to build bytes locally
                    using (MemoryStream ms = new MemoryStream())
                    using (BinaryWriter localWriter = new BinaryWriter(ms))
                    {
                        for (int y = 0; y < grid.Height; y++)
                        {
                            float y0 = y * s;
                            float y1 = (y + 1) * s;

                            for (int z = 0; z < grid.Depth; z++)
                            {
                                if (!grid.GetVoxel(x, y, z)) continue;

                                float z0 = z * s;
                                float z1 = (z + 1) * s;

                                // -X Face
                                if (!grid.GetVoxel(x - 1, y, z))
                                {
                                    WriteTriangle(localWriter, -1, 0, 0, x0, y0, z0, x0, y0, z1, x0, y1, z1);
                                    WriteTriangle(localWriter, -1, 0, 0, x0, y0, z0, x0, y1, z1, x0, y1, z0);
                                }
                                // +X Face
                                if (!grid.GetVoxel(x + 1, y, z))
                                {
                                    WriteTriangle(localWriter, 1, 0, 0, x1, y0, z0, x1, y1, z0, x1, y1, z1);
                                    WriteTriangle(localWriter, 1, 0, 0, x1, y0, z0, x1, y1, z1, x1, y0, z1);
                                }
                                // -Y Face
                                if (!grid.GetVoxel(x, y - 1, z))
                                {
                                    WriteTriangle(localWriter, 0, -1, 0, x0, y0, z0, x1, y0, z0, x1, y0, z1);
                                    WriteTriangle(localWriter, 0, -1, 0, x0, y0, z0, x1, y0, z1, x0, y0, z1);
                                }
                                // +Y Face
                                if (!grid.GetVoxel(x, y + 1, z))
                                {
                                    WriteTriangle(localWriter, 0, 1, 0, x0, y1, z0, x0, y1, z1, x1, y1, z1);
                                    WriteTriangle(localWriter, 0, 1, 0, x0, y1, z0, x1, y1, z1, x1, y1, z0);
                                }
                                // -Z Face
                                if (!grid.GetVoxel(x, y, z - 1))
                                {
                                    WriteTriangle(localWriter, 0, 0, -1, x0, y0, z0, x0, y1, z0, x1, y1, z0);
                                    WriteTriangle(localWriter, 0, 0, -1, x0, y0, z0, x1, y1, z0, x1, y0, z0);
                                }
                                // +Z Face
                                if (!grid.GetVoxel(x, y, z + 1))
                                {
                                    WriteTriangle(localWriter, 0, 0, 1, x0, y0, z1, x1, y0, z1, x1, y1, z1);
                                    WriteTriangle(localWriter, 0, 0, 1, x0, y0, z1, x1, y1, z1, x0, y1, z1);
                                }
                            }
                        }

                        // Flush local buffer to the actual file
                        lock (fileLock)
                        {
                            writer.Write(ms.ToArray());
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Exports a mesh (from Marching Cubes or raw) as binary STL.
        /// </summary>
        public static void ExportMeshToStl(MeshData mesh, string filePath)
        {
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            using (BinaryWriter writer = new BinaryWriter(fs))
            {
                // 80-byte header (can be empty or contain a comment)
                byte[] header = new byte[80];
                // Optional: Encoding.ASCII.GetBytes("Generated by DualIllusionGenerator")
                writer.Write(header);

                // Number of triangles
                writer.Write(mesh.Triangles.Count / 3);

                // Write each triangle
                for (int i = 0; i < mesh.Triangles.Count; i += 3)
                {
                    Vector3 v1 = mesh.Vertices[mesh.Triangles[i]];
                    Vector3 v2 = mesh.Vertices[mesh.Triangles[i + 1]];
                    Vector3 v3 = mesh.Vertices[mesh.Triangles[i + 2]];

                    // Calculate normal (counter-clockwise)
                    Vector3 normal = Vector3.Cross(v2 - v1, v3 - v1);
                    normal = Vector3.Normalize(normal);

                    // Normal (3 floats)
                    writer.Write(normal.X);
                    writer.Write(normal.Y);
                    writer.Write(normal.Z);

                    // Vertex 1
                    writer.Write(v1.X);
                    writer.Write(v1.Y);
                    writer.Write(v1.Z);

                    // Vertex 2
                    writer.Write(v2.X);
                    writer.Write(v2.Y);
                    writer.Write(v2.Z);

                    // Vertex 3
                    writer.Write(v3.X);
                    writer.Write(v3.Y);
                    writer.Write(v3.Z);

                    // Attribute byte count (usually 0)
                    writer.Write((ushort)0);
                }
            }
        }

        private static void WriteTriangle(BinaryWriter writer, float nx, float ny, float nz,
                                          float v1x, float v1y, float v1z,
                                          float v2x, float v2y, float v2z,
                                          float v3x, float v3y, float v3z)
        {
            writer.Write(nx); writer.Write(ny); writer.Write(nz);
            writer.Write(v1x); writer.Write(v1y); writer.Write(v1z);
            writer.Write(v2x); writer.Write(v2y); writer.Write(v2z);
            writer.Write(v3x); writer.Write(v3y); writer.Write(v3z);
            writer.Write((ushort)0);
        }
    }
}
