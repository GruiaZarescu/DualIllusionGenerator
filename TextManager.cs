namespace DualIllusionGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;

    public static class TextManager
    {
        // Characters are always rasterized at this pixel height, no matter what
        // point size was picked in the Font dialog. The Font dialog's size field
        // still controls typeface/style/weight — just not raster resolution.
        // Downstream, uniformScale (in Form1) scales this back down to the
        // physical model size, so more source pixels = smoother curved edges
        // once voxelized, regardless of how small the letter ends up physically.
        private const float RenderPixelHeight = 512f;

        public static List<Stencil> CreateStencilsFromText(string text, Font font)
        {
            List<Stencil> stencils = new List<Stencil>();

            using (Font renderFont = new Font(font.FontFamily, RenderPixelHeight, font.Style, GraphicsUnit.Pixel))
            using (Bitmap measureBmp = new Bitmap(1, 1))
            using (Graphics measureG = Graphics.FromImage(measureBmp))
            {
                measureG.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

                foreach (char c in text)
                {
                    if (char.IsWhiteSpace(c)) continue;

                    SizeF size = measureG.MeasureString(c.ToString(), renderFont);
                    int w = (int)Math.Ceiling(size.Width);
                    int h = (int)Math.Ceiling(size.Height);

                    if (w <= 0 || h <= 0) continue;

                    using (Bitmap charBmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                    using (Graphics g = Graphics.FromImage(charBmp))
                    {
                        g.Clear(Color.Transparent);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        g.DrawString(c.ToString(), renderFont, Brushes.Black, 0, 0);

                        Stencil stencil = StencilManager.CreateFromBitmap(charBmp, autoFix: true);
                        stencils.Add(stencil);
                    }
                }
            }
            return stencils;
        }
    }
}