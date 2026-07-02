using System;
using System.Collections.Generic;
using System.Text;

namespace DualIllusionGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.IO;

    public class AntiAliasingException : Exception
    {
        public AntiAliasingException(string message) : base(message) { }
    }

    public static class StencilManager
    {
        /// <summary>
        /// Creates a Stencil from an image file. Validates alpha and contour connectivity.
        /// </summary>
        public static Stencil CreateFromImage(string filePath, bool autoFix = false, bool debugMode = false)
        {
            using (Bitmap bmp = new Bitmap(filePath))
            {
                BitmapData bmpData = bmp.LockBits(
                    new Rectangle(0, 0, bmp.Width, bmp.Height),
                    ImageLockMode.ReadOnly,
                    PixelFormat.Format32bppArgb
                );

                int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
                byte[] rgbaValues = new byte[bytes];
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbaValues, 0, bytes);
                bmp.UnlockBits(bmpData);

                bool[,] rawMask = new bool[bmp.Width, bmp.Height];
                int minX = bmp.Width;
                int minY = bmp.Height;
                int maxX = -1;
                int maxY = -1;
                bool hasTransparent = false;

                // 1. Process Alpha
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        int pixelIndex = (y * bmpData.Stride) + (x * 4);
                        byte originalAlpha = rgbaValues[pixelIndex + 3];

                        byte alpha = originalAlpha;
                        if (autoFix)
                        {
                            alpha = originalAlpha > 128 ? (byte)255 : (byte)0;
                        }

                        if (alpha == 255)
                        {
                            rawMask[x, y] = true;
                            if (x < minX) minX = x;
                            if (y < minY) minY = y;
                            if (x > maxX) maxX = x;
                            if (y > maxY) maxY = y;
                        }
                        else if (alpha == 0)
                        {
                            hasTransparent = true;
                        }
                        else
                        {
                            throw new AntiAliasingException("Image contains pixels with alpha values between 1 and 254.\nWould you like the app to automatically force these to hard edges (0 or 255)?");
                        }
                    }
                }

                if (!hasTransparent)
                    throw new Exception("Image has no transparent background...");

                if (maxX == -1)
                    throw new Exception("Image is completely transparent...");

                // --- NEW: DUST REMOVAL LOGIC ---
                // We will find ALL connected components, keep the largest one, and delete the rest.
                bool[,] visited = new bool[bmp.Width, bmp.Height];
                List<List<(int x, int y)>> allContours = new List<List<(int x, int y)>>();

                int[] dx = { 0, 0, -1, 1 };
                int[] dy = { -1, 1, 0, 0 };

                // Scan for all contours
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        if (rawMask[x, y] && !visited[x, y])
                        {
                            // Found a new contour, start BFS
                            List<(int x, int y)> currentContour = new List<(int x, int y)>();
                            Queue<(int x, int y)> queue = new Queue<(int, int)>();

                            queue.Enqueue((x, y));
                            visited[x, y] = true;

                            while (queue.Count > 0)
                            {
                                var (cx, cy) = queue.Dequeue();
                                currentContour.Add((cx, cy));

                                for (int i = 0; i < 4; i++)
                                {
                                    int nx = cx + dx[i];
                                    int ny = cy + dy[i];
                                    if (nx >= 0 && nx < bmp.Width && ny >= 0 && ny < bmp.Height)
                                    {
                                        if (rawMask[nx, ny] && !visited[nx, ny])
                                        {
                                            visited[nx, ny] = true;
                                            queue.Enqueue((nx, ny));
                                        }
                                    }
                                }
                            }
                            allContours.Add(currentContour);
                        }
                    }
                }

                // Find the largest contour
                List<(int x, int y)> largestContour = new List<(int x, int y)>();
                foreach (var contour in allContours)
                {
                    if (contour.Count > largestContour.Count)
                    {
                        largestContour = contour;
                    }
                }

                // If the largest contour is still tiny, the image is just noise
                if (largestContour.Count < 50)
                    throw new Exception("No valid solid shape found. The image is mostly noise.");

                // Rebuild a clean mask containing ONLY the largest contour
                // (This automatically deletes all floating dust pixels!)
                bool[,] cleanMask = new bool[bmp.Width, bmp.Height];
                foreach (var (px, py) in largestContour)
                {
                    cleanMask[px, py] = true;
                }

                // --- DEBUG EXPORT ---
                if (debugMode)
                {
                    DEBUG_ExportMask(cleanMask, bmp.Width, bmp.Height, "DEBUG_cleaned_mask.png");
                }

                // 3. Trim to bounding box of the clean mask
                // (Recalculate bounding box since dust removal might have changed edges)
                minX = bmp.Width; minY = bmp.Height; maxX = -1; maxY = -1;
                for (int x = 0; x < bmp.Width; x++)
                {
                    for (int y = 0; y < bmp.Height; y++)
                    {
                        if (cleanMask[x, y])
                        {
                            if (x < minX) minX = x;
                            if (y < minY) minY = y;
                            if (x > maxX) maxX = x;
                            if (y > maxY) maxY = y;
                        }
                    }
                }

                int trimmedWidth = maxX - minX + 1;
                int trimmedHeight = maxY - minY + 1;
                Stencil stencil = new Stencil(trimmedWidth, trimmedHeight);

                for (int x = 0; x < trimmedWidth; x++)
                {
                    for (int y = 0; y < trimmedHeight; y++)
                    {
                        stencil.Mask[x, y] = cleanMask[minX + x, minY + y];
                    }
                }

                return stencil;
            }
        }

        /// <summary>
        /// Creates a Stencil directly from a Bitmap object.
        /// </summary>
        public static Stencil CreateFromBitmap(Bitmap bmp, bool autoFix = true)
        {
            BitmapData bmpData = bmp.LockBits(
                new Rectangle(0, 0, bmp.Width, bmp.Height),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb
            );

            int bytes = Math.Abs(bmpData.Stride) * bmp.Height;
            byte[] rgbaValues = new byte[bytes];
            System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, rgbaValues, 0, bytes);
            bmp.UnlockBits(bmpData);

            bool[,] rawMask = new bool[bmp.Width, bmp.Height];
            int minX = bmp.Width; int minY = bmp.Height; int maxX = -1; int maxY = -1;

            for (int x = 0; x < bmp.Width; x++)
            {
                for (int y = 0; y < bmp.Height; y++)
                {
                    int pixelIndex = (y * bmpData.Stride) + (x * 4);
                    byte originalAlpha = rgbaValues[pixelIndex + 3];
                    byte alpha = autoFix ? (originalAlpha > 128 ? (byte)255 : (byte)0) : originalAlpha;

                    if (alpha == 255)
                    {
                        rawMask[x, y] = true;
                        if (x < minX) minX = x; if (y < minY) minY = y;
                        if (x > maxX) maxX = x; if (y > maxY) maxY = y;
                    }
                    else if (alpha != 0 && !autoFix)
                    {
                        throw new AntiAliasingException("Image contains pixels with alpha values between 1 and 254.");
                    }
                }
            }

            if (maxX == -1) throw new Exception("Character is empty or transparent.");

            // (Skipping dust removal for text, as anti-alias fix usually cleans it up nicely)
            int trimmedWidth = maxX - minX + 1;
            int trimmedHeight = maxY - minY + 1;
            Stencil stencil = new Stencil(trimmedWidth, trimmedHeight);

            for (int x = 0; x < trimmedWidth; x++)
            {
                for (int y = 0; y < trimmedHeight; y++)
                {
                    stencil.Mask[x, y] = rawMask[minX + x, minY + y];
                }
            }
            return stencil;
        }

        // --- DEBUG HELPER ---
        // Explicitly named to ensure we know it's for debugging
        private static void DEBUG_ExportMask(bool[,] mask, int width, int height, string fileName)
        {
            Bitmap debugBmp = new Bitmap(width, height, PixelFormat.Format32bppArgb);
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    if (mask[x, y])
                        debugBmp.SetPixel(x, y, Color.FromArgb(255, 0, 0, 0)); // Solid Black
                    else
                        debugBmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));   // Transparent
                }
            }

            string outputPath = Path.Combine(Path.GetTempPath(), fileName);
            debugBmp.Save(outputPath, ImageFormat.Png);
            debugBmp.Dispose();

            // Open the folder in Windows Explorer so the user can find it easily
            System.Diagnostics.Process.Start("explorer.exe", string.Format("/select,\"{0}\"", outputPath));
        }
    }
}
