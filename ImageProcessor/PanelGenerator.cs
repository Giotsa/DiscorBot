using System;
using System.Collections.Generic;
using System.Text;

using SkiaSharp;

namespace ImageProcessor
{
    class PanelGenerator
    {
        private readonly SKSurface surface;
        private readonly SKCanvas canvas;
        private readonly SKBitmap noise;
        private readonly float blurLevel;

        private readonly SKPaint paint;
        private readonly SKPath clip;

        public PanelGenerator(SKSurface surface, float blurLevel)
        {
            this.blurLevel = blurLevel;
            this.surface = surface;
            canvas = surface.Canvas;
            paint = new SKPaint();
            clip = new SKPath();

            noise = Utils.GetBitmapFromFile("noise_05a.png");
        }

        public void Open()
        {
            paint.ImageFilter = SKImageFilter.CreateBlur(blurLevel, blurLevel, SKShaderTileMode.Mirror);
            paint.IsAntialias = true;

            canvas.Save();
        }

        public void Close()
        {
            canvas.ClipPath(clip, SKClipOperation.Intersect, true);
            canvas.DrawImage(SKImage.FromBitmap(noise), new SKPoint(0, 0));
            canvas.DrawImage(surface.Snapshot(), new SKPoint(0, 0), paint);

            canvas.Restore();
        }

        public void DrawRounRect(SKRoundRect roundRect)
        {
            clip.AddRoundRect(roundRect);
        }

        public void DrawCircle(float x, float y, float radius)
        {
            clip.AddCircle(radius + x, radius + y, radius);
        }
    }
}
