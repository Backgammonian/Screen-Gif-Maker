using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Drawing;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Gif.Components;

namespace GifMaker
{
    public class GifModel : ObservableObject
    {
        private bool _isCreated;
        private bool _isFailed;
        private bool _isStarted;
        private readonly string _path;
        private readonly List<Bitmap> _images;
        private readonly int _delay;

        public GifModel(string path, List<Bitmap> images, int delay, Size originalSize, Rectangle croppingRectangle)
        {
            Name = System.IO.Path.GetFileName(path);
            _path = path;
            _images = images;
            _delay = delay;
            _isStarted = false;
            IsCreated = false;
            IsFailed = false;
            OriginalSize = originalSize;
            CroppingRectangle = croppingRectangle;
        }

        public string Name { get; }
        public int ImagesCount => _images.Count;
        public int Delay => _delay;
        public string Path => _path;
        public Size OriginalSize { get; }
        public Rectangle CroppingRectangle { get; }

        public bool IsCreated
        {
            get => _isCreated;
            private set => SetProperty(ref _isCreated, value);
        }

        public bool IsFailed
        {
            get => _isFailed;
            private set => SetProperty(ref _isFailed, value);
        }

        public bool IsStarted
        {
            get => _isStarted;
            private set => SetProperty(ref _isStarted, value);
        }

        public void CreateGif()
        {
            if (IsStarted)
            {
                return;
            }

            IsStarted = true;

            var task = new Task(() => CreateGifTask());
            task.Start();
        }

        private List<Bitmap> GetCroppedImages()
        {
            var croppedImages = new List<Bitmap>();
            foreach (var image in _images)
            {
                var target = new Bitmap(CroppingRectangle.Width, CroppingRectangle.Height);
                target.SetResolution(96, 96);
                using (var g = Graphics.FromImage(target))
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("Size: {0}, {1}; Resolution: {2}, {3}", target.Width, target.Height, target.HorizontalResolution, target.VerticalResolution));

                    g.DrawImage(image, new Rectangle(0, 0, target.Width, target.Height), CroppingRectangle, GraphicsUnit.Pixel);

                    croppedImages.Add(target);
                }
            }

            return croppedImages;
        }

        private void CreateGifTask()
        {
            try
            {
                var croppedImages = GetCroppedImages();

                var encoder = new AnimatedGifEncoder();
                encoder.Start(_path);
                encoder.SetDelay(_delay);
                encoder.SetRepeat(0);

                for (int i = 0; i < croppedImages.Count; i++)
                {
                    encoder.AddFrame(croppedImages[i]);
                }

                encoder.Finish();

                IsCreated = true;

                foreach (var image in _images)
                {
                    image.Dispose();
                }

                foreach (var image in croppedImages)
                {
                    image.Dispose();
                }
            }
            catch (Exception)
            {
                IsFailed = true;
            }
        }
    }
}
