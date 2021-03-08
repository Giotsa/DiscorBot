using System;
using System.Net.Http;
using System.IO;
using System.Threading.Tasks;

using SkiaSharp;

using DiscordDatabase;

namespace ImageProcessor
{
    public class ProfileGenerator
    {
        readonly float scale = 2.0f;

        // background
        readonly int profileWidth = 1024;
        readonly int profileHeight = 1024;
        readonly SKColor clearColor = SKColors.Transparent;

        SKBitmap background;
        SKBitmap avatar;

        readonly SKImageInfo info;
        readonly SKSurface surface;
        readonly SKCanvas canvas;

        readonly PanelGenerator panel;

        public ProfileGenerator()
        {
            info = new SKImageInfo(profileWidth, profileHeight);
            surface = SKSurface.Create(info);
            canvas = surface.Canvas;

            panel = new PanelGenerator(surface, blurLevel: 16);
        }

        public async Task<Stream> DrawProfile(User user)
        {
            // get bitmaps
            background = await Utils.GetBitmapFromUrl(user.BackgroundUrl);
            avatar = await Utils.GetBitmapFromUrl(user.AvatarUrl);

            // clear the canvas
            canvas.Clear(clearColor);

            // draw background
            {
                using (SKPaint paint = new SKPaint())
                {
                    // setup paint
                    paint.FilterQuality = SKFilterQuality.High;

                    // resize, crop and covert to image background
                    SKImage BackgroundImage = PrepareBackground(background);

                    // draw background
                    canvas.DrawImage(BackgroundImage, SKRect.Create(0, 0, profileWidth, profileHeight), paint);
                }
            }

            // draw panels
            {
                // setups paint and canvas
                panel.Open();

                // main panel
                {
                    var position = new SKPoint(20 * scale, 150 * scale);
                    var size = new SKSize(472 * scale, 342 * scale);
                    var roundRect = new SKRoundRect(SKRect.Create(position, size), 32 * scale);
                    panel.DrawRounRect(roundRect);
                }

                // avatar border
                {
                    var position = new SKPoint(x: 192 * scale, y: 82 * scale);
                    var radius = 64 * scale;
                    var border = 16;

                    panel.DrawCircle(position.X - border, position.Y - border, radius + border);
                }

                // draw panels
                panel.Close();
            }

            // draw avatar
            {
                // avatar information
                var position = new SKPoint(x: 192 * scale, y: 82 * scale);
                var size = new SKSize(width: 128 * scale, height: 128 * scale);
                var radius = 64 * scale;

                using (var paint = new SKPaint())
                {
                    // setup paint
                    paint.IsAntialias = true;
                    paint.FilterQuality = SKFilterQuality.High;

                    // save canvas
                    canvas.Save();

                    // create clip and aplay it to canvas
                    using (SKPath clip = new SKPath())
                    {
                        clip.AddCircle(radius + position.X, radius + position.Y, radius);
                        canvas.ClipPath(clip, SKClipOperation.Intersect, true);
                    }

                    // draw avatar
                    canvas.DrawBitmap(avatar, SKRect.Create(position, size), paint);

                    // restore canvas
                    canvas.Restore();
                }
            }

            // draw username
            {
                using (var paint = new SKPaint())
                {
                    // setup paint
                    paint.TextSize = (int)(48 * scale);
                    paint.Color = SKColors.White;
                    paint.IsAntialias = true;

                    // calculate position
                    float UsernameLength = paint.MeasureText($"{ user.Username }#{ user.Discriminator }");
                    float x = ((profileWidth - UsernameLength) / 2);
                    float y = 250 * scale;

                    // draw username
                    paint.Typeface = SKTypeface.FromFile("Common/Assets/Fonts/Roboto-Regular.ttf");
                    canvas.DrawText(user.Username, x, y, paint);

                    // recalculate position for discriminator
                    float offset = paint.MeasureText($"{ user.Username }");
                    x += offset;

                    // draw discriminator
                    paint.Typeface = SKTypeface.FromFile("Common/Assets/Fonts/Roboto-Thin.ttf");
                    canvas.DrawText($"#{ user.Discriminator }", x, y, paint);
                }
            }


            // return surface as stream
            return surface.Snapshot().Encode().AsStream();
        }

        private SKImage PrepareBackground(SKBitmap background)
        {
            // calculate aspect ratio
            float bgWidth = background.Width;
            float bgHeight = background.Height;
            float ratio = bgWidth / bgHeight;

            if (ratio > 0)
            {
                // image is horizontal
                // resize to fit background
                int width = (int)(profileWidth * ratio);
                int height = profileHeight;
                background = background.Resize(new SKSizeI(width, height), SKFilterQuality.High);
            }
            else
            {
                // image is vertical
                // resize to fit background
                int width = profileWidth;
                int height = (int)(profileHeight * ratio);
                background = background.Resize(new SKSizeI(width, height), SKFilterQuality.High);
            }

            // crop image
            // calculate subset rectangle
            int subsetX = (background.Width - profileWidth) / 2;
            int subsetY = (background.Height - profileHeight) / 2;
            int subsetWidth = subsetX + profileWidth;
            int subsetHeight = subsetY + profileHeight;

            // convert bitmap to image
            SKImage image = SKImage.FromBitmap(background);

            // subset image from background
            SKRectI subsetRect = new SKRectI(subsetX, subsetY, subsetWidth, subsetHeight);
            return image.Subset(subsetRect);
        }

    }
}
