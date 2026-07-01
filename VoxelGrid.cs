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
    public void ApplyStencil(Stencil stencil, CarvePlane plane, CarveOperation operation)
    {
        if (plane != CarvePlane.Top)
            throw new NotImplementedException("Only Top plane is supported in this initial version.");

        // 1. Calculate Uniform Scale
        float scaleX = (float)Width / stencil.Width;
        float scaleY = (float)Height / stencil.Height;
        float uniformScale = Math.Min(scaleX, scaleY);

        float scaledWidth = stencil.Width * uniformScale;
        float scaledHeight = stencil.Height * uniformScale;

        // 2. Calculate Centering Offset
        float offsetX = (Width - scaledWidth) / 2.0f;
        float offsetY = (Height - scaledHeight) / 2.0f;

        // 3. Iterate over the top face (X, Y)
        Parallel.For(0, Width, x =>
        {
            for (int y = 0; y < Height; y++)
            {
                // FIX: Map the CENTER of the voxel (x + 0.5f) to the stencil
                float gridCenterX = x + 0.5f;
                float gridCenterY = y + 0.5f;

                int stencilX = (int)((gridCenterX - offsetX) / uniformScale);
                int stencilY = (int)((gridCenterY - offsetY) / uniformScale);

                if (stencil.IsInBounds(stencilX, stencilY))
                {
                    if (stencil.Mask[stencilX, stencilY])
                    {
                        for (int z = 0; z < Depth; z++)
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