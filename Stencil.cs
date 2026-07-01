using System;
using System.Collections.Generic;
using System.Text;

namespace DualIllusionGenerator
{
    public class Stencil
    {
        public int Width { get; }
        public int Height { get; }

        // 2D array of booleans. true = solid, false = empty
        public bool[,] Mask { get; }

        public Stencil(int width, int height)
        {
            Width = width;
            Height = height;
            Mask = new bool[width, height];
        }

        // Helper to check if a coordinate is within the stencil bounds
        public bool IsInBounds(int x, int y)
        {
            return x >= 0 && x < Width && y >= 0 && y < Height;
        }
    }
}
