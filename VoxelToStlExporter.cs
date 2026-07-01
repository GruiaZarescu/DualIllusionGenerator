using System;
using System.Collections.Generic;
using System.Text;

namespace DualIllusionGenerator
{
    using System.IO;
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
