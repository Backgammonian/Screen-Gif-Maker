using System.Windows.Media.Imaging;
using System.Drawing;
using System.Diagnostics;
using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;

namespace GifMaker
{
    public class ScreenshotModel : ObservableObject
    {
        private bool _isSkipped;
        private BitmapImage _thumbnail;
        private bool _isThumbnailCreated;

        public ScreenshotModel(string path, Bitmap bitmap)
        {
            ImagePath = path;
            Name = Path.GetFileName(path);
            IsSkipped = false;
            IsThumbnailCreated = false;
            Size = new Size(bitmap.Width, bitmap.Height);

            CreateThumbnail(bitmap);
        }

        public ScreenshotModel(ScreenshotModel model)
        {
            ImagePath = model.ImagePath;
            Name = model.Name;
            IsSkipped = model.IsSkipped;
            IsThumbnailCreated = model.IsThumbnailCreated;
            Size = new Size(model.Size.Width, model.Size.Height);
            Thumbnail = model.Thumbnail.Clone();
        }

        public string Name { get; }
        public string ImagePath { get; }
        public Size Size { get; }

        public bool IsSkipped
        {
            get => _isSkipped;
            set => SetProperty(ref _isSkipped, value);
        }

        public bool IsThumbnailCreated
        {
            get => _isThumbnailCreated;
            private set => SetProperty(ref _isThumbnailCreated, value);
        }

        public BitmapImage Thumbnail
        {
            get => _thumbnail;
            private set => SetProperty(ref _thumbnail, value);
        }

        private void CreateThumbnail(Bitmap bitmap)
        {
            if (IsThumbnailCreated)
            {
                return;
            }

            var smallImage = bitmap.Resize(new Size(bitmap.Width / 5, bitmap.Height / 5));

            Thumbnail = smallImage.ToBitmapImage();
            IsThumbnailCreated = true;

            Debug.WriteLine(string.Format("(CreateThumbnail) Size: {2}, {3}; Resolution: {0}, {1}", Thumbnail.DpiX, Thumbnail.DpiY, Thumbnail.Width, Thumbnail.Height));
        }
    }
}