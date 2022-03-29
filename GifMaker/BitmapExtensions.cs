using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Media.Imaging;

namespace GifMaker
{
    public static class BitmapExtensions
    {
        public static Bitmap Resize(this Bitmap source, Size newSize)
        {
            var sourceWidth = source.Width;
            var sourceHeight = source.Height;

            var nPercentW = newSize.Width / (float)sourceWidth;
            var nPercentH = newSize.Height / (float)sourceHeight;

            var nPercent = nPercentH < nPercentW ? nPercentH : nPercentW;
                
            int destinationWidth = (int)(sourceWidth * nPercent);
            var destinationHeight = (int)(sourceHeight * nPercent);

            var destination = new Bitmap(destinationWidth, destinationHeight);
            using var g = Graphics.FromImage(destination);
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.DrawImage(source, 0, 0, destinationWidth, destinationHeight);

            return destination;
        }

        public static BitmapImage ToBitmapImage(this Bitmap image)
        {
            using MemoryStream memory = new MemoryStream();
            image.Save(memory, ImageFormat.Jpeg);
            memory.Position = 0;
            var bitmapImage = new BitmapImage();
            bitmapImage.BeginInit();
            bitmapImage.StreamSource = memory;
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.EndInit();

            return bitmapImage;
        }

        public static Bitmap Crop(this Bitmap image, Rectangle croppingRectangle)
        {
            var target = new Bitmap(croppingRectangle.Width, croppingRectangle.Height);
            target.SetResolution(96, 96);

            using var g = Graphics.FromImage(target);
            g.DrawImage(image, new Rectangle(0, 0, target.Width, target.Height), croppingRectangle, GraphicsUnit.Pixel);

            return target;
        }
    }
}
