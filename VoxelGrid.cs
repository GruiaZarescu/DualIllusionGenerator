using DualIllusionGenerator;
using System;
using System.Threading;

public class VoxelGrid
{
    public enum CarveOperation
    {
        Extrude, // Set voxels to 1
        Cut      // Set voxels to 0
    }

    public enum CarvePlane
    {
        Front,
        Top
    }

    public int Width { get; }
    public int Height { get; }
    public int Depth { get; }

    public float OriginX { get; set; }
    public float OriginY { get; set; }
    public float OriginZ { get; set; }

    public float VoxelSize { get; set; }

    // Upgraded to uint[] (32-bit) to allow Interlocked operations for thread safety!
    private uint[] _data;

    public VoxelGrid(int width, int height, int depth, float voxelSize = 1.0f)
    {
        Width = width;
        Height = height;
        Depth = depth;
        VoxelSize = voxelSize;

        long totalVoxels = (long)width * (long)height * (long)depth;

        // Calculate required 32-bit elements (32 voxels per uint)
        long elementCount = (totalVoxels + 31) / 32;

        if (elementCount > Array.MaxLength)
            throw new OutOfMemoryException("Voxel grid exceeds maximum .NET array size.");

        _data = new uint[elementCount];

        OriginX = 0; OriginY = 0; OriginZ = 0;
    }

    private long GetFlatIndex(int x, int y, int z)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height || z < 0 || z >= Depth)
            return -1;

        return (long)x + ((long)y * Width) + ((long)z * Width * Height);
    }

    public bool GetVoxel(int x, int y, int z)
    {
        long index = GetFlatIndex(x, y, z);
        if (index == -1) return false;

        long elementIndex = index >> 5; // Equivalent to index / 32
        int bitOffset = (int)(index & 31); // Equivalent to index % 32

        return (_data[elementIndex] & (1u << bitOffset)) != 0;
    }

    public void SetVoxel(int x, int y, int z, bool value)
    {
        long index = GetFlatIndex(x, y, z);
        if (index == -1) return;

        long elementIndex = index >> 5;
        int bitOffset = (int)(index & 31);

        if (value)
        {
            // Thread-safe atomic write
            Interlocked.Or(ref _data[elementIndex], 1u << bitOffset);
        }
        else
        {
            // Thread-safe atomic write
            Interlocked.And(ref _data[elementIndex], ~(1u << bitOffset));
        }
    }

    /// <summary>
    /// Applies a 2D stencil to the voxel grid.
    /// </summary>
    /// <summary>
    /// Applies a 2D stencil to the voxel grid.
    /// </summary>
    public void ApplyStencil(Stencil stencil, CarvePlane plane, CarveOperation operation, bool stretchToFill, float paddingPercent, float offsetX, float offsetY)
    {
        float scaleX, scaleY, uniformScale, scaledWidth, scaledHeight, offsetXFinal, offsetYFinal;

        // Calculate padding multiplier (e.g., 10% padding means we use 90% of the space)
        float effectivePadding = (operation == CarveOperation.Cut) ? paddingPercent : 0f;
        float padMultiplier = 1.0f - (effectivePadding / 100.0f);

        // 1. Calculate Scaling and Offsets based on the Plane
        if (plane == CarvePlane.Top)
        {
            // Top plane maps to X and Y. Extrudes down Z.
            // Apply padding to the target area
            float targetWidth = Width * padMultiplier;
            float targetHeight = Height * padMultiplier;

            scaleX = targetWidth / stencil.Width;
            scaleY = targetHeight / stencil.Height;
            uniformScale = stretchToFill ? Math.Max(scaleX, scaleY) : Math.Min(scaleX, scaleY);

            scaledWidth = stencil.Width * uniformScale;
            scaledHeight = stencil.Height * uniformScale;

            // Apply user offsets (in voxels)
            offsetXFinal = ((Width - scaledWidth) / 2.0f) + offsetX;
            offsetYFinal = ((Height - scaledHeight) / 2.0f) + offsetY;
        }
        else // Front plane
        {
            // Front plane maps to X and Z. Extrudes back Y.
            // Apply padding to the target area
            float targetWidth = Width * padMultiplier;
            float targetHeight = Depth * padMultiplier;

            scaleX = targetWidth / stencil.Width;
            scaleY = targetHeight / stencil.Height;
            uniformScale = stretchToFill ? Math.Max(scaleX, scaleY) : Math.Min(scaleX, scaleY);

            scaledWidth = stencil.Width * uniformScale;
            scaledHeight = stencil.Height * uniformScale;

            offsetXFinal = ((Width - scaledWidth) / 2.0f) + offsetX;
            offsetYFinal = ((Depth - scaledHeight) / 2.0f) + offsetY; // Centering on Z axis
        }

        // 2. Iterate over the grid
        Parallel.For(0, Width, x =>
        {
            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    int stencilX, stencilY;

                    if (plane == CarvePlane.Top)
                    {
                        float gridCenterX = x + 0.5f;
                        float gridCenterY = y + 0.5f;
                        stencilX = (int)((gridCenterX - offsetXFinal) / uniformScale);
                        stencilY = (int)((gridCenterY - offsetYFinal) / uniformScale);
                    }
                    else // Front plane
                    {
                        float gridCenterX = x + 0.5f;
                        float gridCenterZ = z + 0.5f;
                        stencilX = (int)((gridCenterX - offsetXFinal) / uniformScale);
                        stencilY = (int)((gridCenterZ - offsetYFinal) / uniformScale);
                    }

                    if (stencil.IsInBounds(stencilX, stencilY))
                    {
                        if (stencil.Mask[stencilX, stencilY])
                        {
                            if (operation == CarveOperation.Extrude)
                                SetVoxel(x, y, z, true);
                            else if (operation == CarveOperation.Cut)
                                SetVoxel(x, y, z, false);
                        }
                    }
                }
            }
        });
    }

    /// <summary>
    /// Applies a text stencil at a specific angle within a designated slot.
    /// </summary>
    /// <summary>
    /// Applies a text stencil at a specific angle within a designated slot.
    /// </summary>
    /// <summary>
    /// Applies a text stencil at a specific angle within a designated slot.
    /// </summary>
    /// <summary>
    /// Applies a text stencil at a specific angle within a designated slot.
    /// </summary>
    public void ApplyTextStencil(Stencil stencil, float angleDeg, CarveOperation operation, int slotIndex, int totalSlots)
    {
        float rad = angleDeg * (float)Math.PI / 180.0f;
        float cosA = (float)Math.Cos(rad);
        float sinA = (float)Math.Sin(rad);

        float slotWidth = (float)Width / totalSlots;
        float slotCenterX = (slotIndex * slotWidth) + (slotWidth / 2.0f);
        float slotCenterY = Depth / 2.0f;
        float slotCenterZ = Height / 2.0f;

        float scaleX = slotWidth / stencil.Width;
        float scaleY = (float)Height / stencil.Height;
        float scale = Math.Min(scaleX, scaleY) * 0.9f;

        // Calculate exact max depth based on the actual physical bounds of the text
        float u_half = (stencil.Width / 2.0f) * scale;
        float maxDepth = float.MaxValue;

        if (sinA > 0.0001f)
        {
            float minX_start = slotCenterX - u_half * Math.Abs(cosA);
            maxDepth = Math.Min(maxDepth, minX_start / sinA);
        }
        else if (sinA < -0.0001f)
        {
            float maxX_start = slotCenterX + u_half * Math.Abs(cosA);
            maxDepth = Math.Min(maxDepth, (Width - maxX_start) / -sinA);
        }

        if (cosA > 0.0001f)
        {
            float maxY_start = slotCenterY + u_half * Math.Abs(sinA);
            maxDepth = Math.Min(maxDepth, (Depth - maxY_start) / cosA);
        }
        else if (cosA < -0.0001f)
        {
            float minY_start = slotCenterY - u_half * Math.Abs(sinA);
            maxDepth = Math.Min(maxDepth, -minY_start / cosA);
        }

        maxDepth -= 1.0f; // Keep 1 voxel away from walls to be safe
        if (maxDepth < 0) maxDepth = 0;

        Parallel.For(0, Width, x =>
        {
            for (int y = 0; y < Depth; y++)
            {
                for (int z = 0; z < Height; z++)
                {
                    float localX = x - slotCenterX;
                    float localY = y - slotCenterY;
                    float localZ = z - slotCenterZ;

                    float d = -localX * sinA + localY * cosA;

                    if (d < 0 || d > maxDepth) continue;

                    float u = localX * cosA + localY * sinA;
                    float v = localZ;

                    int stencilX = (int)((u / scale) + (stencil.Width / 2.0f));
                    int stencilY = (int)((-v / scale) + (stencil.Height / 2.0f));

                    if (stencil.IsInBounds(stencilX, stencilY) && stencil.Mask[stencilX, stencilY])
                    {
                        if (operation == CarveOperation.Extrude)
                            SetVoxel(x, y, z, true);
                        else if (operation == CarveOperation.Cut)
                            SetVoxel(x, y, z, false);
                    }
                }
            }
        });
    }

    public void FillAll()
    {
        // uint.MaxValue means all 32 bits are set to 1
        for (long i = 0; i < _data.Length; i++)
        {
            _data[i] = uint.MaxValue;
        }
    }

    public void ClearAll()
    {
        Array.Clear(_data, 0, _data.Length);
    }

    public (float X, float Y, float Z) GetVoxelCenter(int x, int y, int z)
    {
        return (
            OriginX + (x * VoxelSize) + (VoxelSize / 2f),
            OriginY + (y * VoxelSize) + (VoxelSize / 2f),
            OriginZ + (z * VoxelSize) + (VoxelSize / 2f)
        );
    }
}