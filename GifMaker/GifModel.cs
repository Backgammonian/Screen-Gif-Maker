using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.IO;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Gif.Components;

namespace GifMaker
{
    public class GifModel : ObservableObject
    {
        private bool _isCreated;
        private bool _isFailed;
        private bool _isStarted;
        private bool _isCancelled;
        private readonly List<string> _imagesPaths;
        private readonly CancellationTokenSource _tokenSource;

        public GifModel(string path, List<string> imagesPaths, int delay, Size originalSize, Rectangle croppingRectangle)
        {
            _imagesPaths = imagesPaths;
            _tokenSource = new CancellationTokenSource();

            Name = Path.GetFileName(path);
            ImagePath = path;
            Delay = delay;
            IsStarted = false;
            IsCreated = false;
            IsFailed = false;
            OriginalSize = originalSize;
            CroppingRectangle = croppingRectangle;
        }

        public string Name { get; }
        public int ImagesCount => _imagesPaths.Count;
        public int Delay { get; }
        public string ImagePath { get; }
        public Size OriginalSize { get; }
        public Rectangle CroppingRectangle { get; }
        public bool IsRunning => IsStarted && !(IsCancelled || IsFailed) && !IsCreated;

        public bool IsCreated
        {
            get => _isCreated;
            private set
            {
                SetProperty(ref _isCreated, value);
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public bool IsFailed
        {
            get => _isFailed;
            private set
            {
                SetProperty(ref _isFailed, value);
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public bool IsStarted
        {
            get => _isStarted;
            private set
            {
                SetProperty(ref _isStarted, value);
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public bool IsCancelled
        {
            get => _isCancelled;
            private set
            {
                SetProperty(ref _isCancelled, value);
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public async Task CreateGif()
        {
            if (IsStarted)
            {
                return;
            }

            IsStarted = true;

            await Task.Run(() => CreateGifTask(_tokenSource.Token));
        }

        private void CreateGifTask(CancellationToken token)
        {
            try
            {
                var encoder = new AnimatedGifEncoder();
                encoder.Start(ImagePath);
                encoder.SetDelay(Delay);
                encoder.SetRepeat(0);

                for (int i = 0; i < _imagesPaths.Count; i++)
                {
                    if (token.IsCancellationRequested)
                    {
                        return;
                    }

                    //if one of the images is missing, just skip it
                    try
                    {
                        using var image = new Bitmap(_imagesPaths[i]);
                        using var croppedImage = image.Crop(CroppingRectangle);

                        encoder.AddFrame(croppedImage);
                    }
                    catch (Exception)
                    {
                        continue;
                    }
                }

                encoder.Finish();

                IsCreated = true;
            }
            catch (Exception)
            {
                IsFailed = true;
            }
        }

        public void Cancel()
        {
            IsCancelled = true;
            _tokenSource.Cancel();
        }
    }
}
