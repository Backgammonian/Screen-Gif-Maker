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
            CropOrigin = croppingRectangle.Location;
            CroppedSize = croppingRectangle.Size;
        }

        public string Name { get; }
        public int ImagesCount => _images.Count;
        public int Delay => _delay;
        public string Path => _path;
        public Size OriginalSize { get; }
        public Point CropOrigin { get; }
        public Size CroppedSize { get; }

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

        private void CreateGifTask()
        {
            try
            {
                var encoder = new AnimatedGifEncoder();
                encoder.Start(_path);
                encoder.SetDelay(_delay);
                encoder.SetRepeat(0);

                for (int i = 0; i < _images.Count; i++)
                {
                    encoder.AddFrame(_images[i]);
                }

                encoder.Finish();

                IsCreated = true;

                foreach (var bitmap in _images)
                {
                    bitmap.Dispose();
                }
            }
            catch (Exception)
            {
                IsFailed = true;
            }
        }
    }
}
