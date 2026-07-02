using System;
using System.Collections.Generic;
using System.Text;

namespace DualIllusionGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;

    public static class TextManager
    {
        public static List<Stencil> CreateStencilsFromText(string text, Font font)
        {
            List<Stencil> stencils = new List<Stencil>();

            using (Bitmap measureBmp = new Bitmap(1, 1))
            using (Graphics measureG = Graphics.FromImage(measureBmp))
            {
                foreach (char c in text)
                {
                    if (char.IsWhiteSpace(c)) continue; // Skip spaces

                    // Measure the character to get its exact bitmap size
                    SizeF size = measureG.MeasureString(c.ToString(), font);
                    int w = (int)Math.Ceiling(size.Width);
                    int h = (int)Math.Ceiling(size.Height);

                    if (w <= 0 || h <= 0) continue;

                    using (Bitmap charBmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                    using (Graphics g = Graphics.FromImage(charBmp))
                    {
                        g.Clear(Color.Transparent);
                        g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                        g.DrawString(c.ToString(), font, Brushes.Black, 0, 0);

                        // Convert to stencil (autoFix handles the text anti-aliasing)
                        Stencil stencil = StencilManager.CreateFromBitmap(charBmp, autoFix: true);
                        stencils.Add(stencil);
                    }
                }
            }
            return stencils;
        }
    }
}
