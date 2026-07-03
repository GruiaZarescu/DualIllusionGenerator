namespace DualIllusionGenerator
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.Drawing.Imaging;

    public static class TextManager
    {

        public static List<Stencil> CreateStencilsFromText(string text, Font font,float renderPixelHeight =512f)
        {
            if (renderPixelHeight <= 0f) renderPixelHeight = 64f;

            List<Stencil> stencils = new List<Stencil>();

            using (Font renderFont = new Font(font.FontFamily, renderPixelHeight , font.Style, GraphicsUnit.Pixel))
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
                        stencil.ComputeTrueBottom();
                        stencils.Add(stencil);
                    }
                }
            }
            return stencils;
        }

        /// <summary>
        /// Renders an entire text string into a single Stencil for Dual Image mode.
        /// </summary>
        public static Stencil CreateWholeTextStencil(string text, Font font,float renderPixelHeight = 512f)
        {
            if (string.IsNullOrWhiteSpace(text))
                throw new Exception("Text is empty.");
            if(renderPixelHeight <= 0) renderPixelHeight = 64f;

            // Force a consistent render resolution for clean edges
            using (Font renderFont = new Font(font.FontFamily, renderPixelHeight, font.Style, GraphicsUnit.Pixel))
            using (Bitmap measureBmp = new Bitmap(1, 1))
            using (Graphics measureG = Graphics.FromImage(measureBmp))
            {
                measureG.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                SizeF size = measureG.MeasureString(text, renderFont);
                int w = (int)Math.Ceiling(size.Width);
                int h = (int)Math.Ceiling(size.Height);

                if (w <= 0 || h <= 0) throw new Exception("Text rendered to 0 size.");

                using (Bitmap textBmp = new Bitmap(w, h, PixelFormat.Format32bppArgb))
                using (Graphics g = Graphics.FromImage(textBmp))
                {
                    g.Clear(Color.Transparent);
                    g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
                    g.DrawString(text, renderFont, Brushes.Black, 0, 0);

                    // Convert the whole string to a single stencil
                    return StencilManager.CreateFromBitmap(textBmp, autoFix: true);
                }
            }
        }

    }
}