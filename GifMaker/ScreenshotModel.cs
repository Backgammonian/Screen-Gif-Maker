using System.IO;
using System.Windows.Media.Imaging;
using System.Drawing;
using System.Drawing.Imaging;
using System.Diagnostics;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GifMaker
{
    public class ScreenshotModel : ObservableObject
    {
        private string _name;
        private bool _isSkipped;
        private BitmapImage _bitmapImage;
        private bool _isConverted;

        public ScreenshotModel(string name, Bitmap bitmap)
        {
            Name = name;
            IsSkipped = false;
            Bitmap = bitmap;
            IsConverted = false;
        }

        public ScreenshotModel(ScreenshotModel model)
        {
            Name = model.Name;
            IsSkipped = model.IsSkipped;
            Bitmap = model.Bitmap;
            IsConverted = model.IsConverted;
            if (!IsConverted)
            {
                ConvertToBitmapImage();
            }
            else
            {
                Image = model.Image;
            }
        }

        public Bitmap Bitmap { get; }

        public string Name
        {
            get => _name;
            private set => SetProperty(ref _name, value);
        }

        public bool IsSkipped
        {
            get => _isSkipped;
            set => SetProperty(ref _isSkipped, value);
        }

        public bool IsConverted
        {
            get => _isConverted;
            private set => SetProperty(ref _isConverted, value);
        }

        public BitmapImage Image
        {
            get => _bitmapImage;
            private set => SetProperty(ref _bitmapImage, value);
        }

        public void ConvertToBitmapImage()
        {
            if (IsConverted)
            {
                return;
            }

            using MemoryStream memory = new MemoryStream();
            Bitmap.Save(memory, ImageFormat.Bmp);
            memory.Position = 0;
            var bitmapimage = new BitmapImage();
            bitmapimage.BeginInit();
            bitmapimage.StreamSource = memory;
            bitmapimage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapimage.EndInit();

            Image = bitmapimage;
            _isConverted = true;

            Debug.WriteLine(string.Format("(ConvertToBitmapImage) Size: {2}, {3}; Resolution: {0}, {1}", Image.DpiX, Image.DpiY, Image.Width, Image.Height));
        }
    }
}