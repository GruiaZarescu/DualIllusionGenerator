using DualIllusionGenerator;
using System;
using System.Threading;

public class VoxelGrid
{
    public enum CarveOperation
    {
        Extrude, // Set voxels to 1
        Cut,     // Set voxels to 0
        Intersect // Set voxels to 0 if stencil is empty, leave alone if stencil is solid
    }

    public enum CarvePlane
    {
        Front,
        Top
    }

    public struct LetterBounds
    {
        public float MaxDepth;     // valid only for the Extrude letter this was computed from
        public float HalfWidthX;   // exact X half-width the letter's rectangle occupies once rotated
    }

    /// <summary>
    /// Computes exactly how much of the grid's X-axis a letter's rotated stencil will
    /// occupy once it's angle/scale/depth-bounded for Extrude. This is the number the
    /// paired Intersect call needs — not slotWidth/2, not its own maxDepth — to know
    /// precisely which voxels it's allowed to touch.
    /// </summary>
    public LetterBounds ComputeLetterBounds(Stencil stencil, float angleDeg, float uniformScale, float slotWidth)
    {
        float rad = angleDeg * (float)Math.PI / 180.0f;
        float cosA = (float)Math.Cos(rad);
        float sinA = (float)Math.Sin(rad);

        float u_half = (stencil.Width / 2.0f) * uniformScale;

        float maxDepthY = float.MaxValue;
        if (Math.Abs(cosA) > 0.0001f)
            maxDepthY = ((Height / 2.0f) - (u_half * Math.Abs(sinA))) / Math.Abs(cosA);

        float maxDepthX = float.MaxValue;
        if (Math.Abs(sinA) > 0.0001f)
            maxDepthX = ((slotWidth / 2.0f) - (u_half * Math.Abs(cosA))) / Math.Abs(sinA);

        float maxDepth = Math.Min(maxDepthY, maxDepthX) - 1.0f;
        if (maxDepth < 0) maxDepth = 0;

        // The rectangle spans u in [-u_half, u_half] and d in [-maxDepth, maxDepth].
        // localX = u*cosA - d*sinA, so its max |localX| (a corner of the rectangle) is:
        float halfWidthX = (u_half * Math.Abs(cosA)) + (maxDepth * Math.Abs(sinA));

        return new LetterBounds { MaxDepth = maxDepth, HalfWidthX = halfWidthX };
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
    /// confinementHalfWidthX is a hard fence on the grid's X-axis: voxels outside it are
    /// never touched. For Extrude, pass the letter's own HalfWidthX (from ComputeLetterBounds) —
    /// it's an exact match for the d-bound, not an approximation. For Intersect, pass the
    /// PAIRED Extrude letter's HalfWidthX — this is what stops Intersect from reaching into
    /// neighboring slots, and it's also why Intersect no longer needs (or uses) any depth/d bound
    /// of its own: outside this fence, Extrude never placed anything, so there's nothing to erase.
    /// </summary>
    public void ApplyTextStencil(Stencil stencil, float angleDeg, CarveOperation operation, float slotCenterX, float uniformScale, float slotWidth, float confinementHalfWidthX, float zOffsetVoxels = 0f)
    {
        float rad = angleDeg * (float)Math.PI / 180.0f;
        float cosA = (float)Math.Cos(rad);
        float sinA = (float)Math.Sin(rad);

        float slotCenterY = (float)Height / 2.0f;
        float slotCenterZ = zOffsetVoxels + ((Depth - zOffsetVoxels) / 2.0f);

        float maxDepth = 0f;
        if (operation == CarveOperation.Extrude)
        {
            float u_half = (stencil.Width / 2.0f) * uniformScale;

            float maxDepthY = float.MaxValue;
            if (Math.Abs(cosA) > 0.0001f)
                maxDepthY = ((Height / 2.0f) - (u_half * Math.Abs(sinA))) / Math.Abs(cosA);

            float maxDepthX = float.MaxValue;
            if (Math.Abs(sinA) > 0.0001f)
                maxDepthX = ((slotWidth / 2.0f) - (u_half * Math.Abs(cosA))) / Math.Abs(sinA);

            maxDepth = Math.Min(maxDepthY, maxDepthX) - 1.0f;
            if (maxDepth < 0) maxDepth = 0;
        }

        Parallel.For(0, Width, x =>
        {
            // Hard fence, independent of either letter's rotation. This is what actually
            // confines the operation to its own slot — depth-bounding along a diagonal
            // was never the right tool for that job.
            if (Math.Abs((x + 0.5f) - slotCenterX) > confinementHalfWidthX) return;

            for (int y = 0; y < Height; y++)
            {
                for (int z = 0; z < Depth; z++)
                {
                    float localX = x - slotCenterX;
                    float localY = y - slotCenterY;
                    float localZ = z - slotCenterZ;

                    if (operation == CarveOperation.Extrude)
                    {
                        float d = -localX * sinA + localY * cosA;
                        if (d < -maxDepth || d > maxDepth) continue;
                    }

                    float u = localX * cosA + localY * sinA;
                    float v = localZ;

                    int stencilX = (int)((u / uniformScale) + (stencil.Width / 2.0f));
                    int stencilY = (int)((-v / uniformScale) + (stencil.Height / 2.0f));

                    bool isSolid = stencil.IsInBounds(stencilX, stencilY) && stencil.Mask[stencilX, stencilY];

                    if (operation == CarveOperation.Extrude)
                    {
                        if (isSolid) SetVoxel(x, y, z, true);
                    }
                    else if (operation == CarveOperation.Intersect)
                    {
                        if (!isSolid) SetVoxel(x, y, z, false);
                    }
                }
            }
        });
    }

    /// <summary>
    /// Fills a solid slab of the given thickness at the bottom (z=0), across the
    /// full X/Y footprint, so physically separate carved pieces (like letters)
    /// have something to stand on and print as one connected part.
    /// </summary>
    public void AddBasePlate(int thicknessVoxels)
    {
        if (thicknessVoxels <= 0) return;
        int zMax = Math.Min(thicknessVoxels, Depth);
        Parallel.For(0, Width, x =>
        {
            for (int y = 0; y < Height; y++)
                for (int z = 0; z < zMax; z++)
                    SetVoxel(x, y, z, true);
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