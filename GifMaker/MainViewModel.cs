using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Threading;
using System.Reflection;
using System.Collections.Generic;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using CroppingImageLibrary;
using Windows = System.Windows;

namespace GifMaker
{
    public class MainViewModel : ObservableObject
    {
        private BitmapImage _preview;
        private readonly BitmapImage _defaultPreview;
        private ScreenshotModel _selectedScreenshot;
        private readonly DispatcherTimer _timer;
        private int _interval;
        private bool _isNotRunning;
        private readonly Size _screenSize;
        private DateTime _startTime;
        private int _currentScreenshotNumber;
        private readonly List<string> _folderNames;
        private string _currentFolderName;
        private CroppingWindow _croppingWindow;
        private bool _canCreateGif;
        private int _delay;
        private bool _isNotFullImage;
        private bool _isFullImage;
        private readonly float _originalDpiX;
        private readonly float _originalDpiY;
        private bool _isScreenshotMissing;

        public MainViewModel()
        {
            _screenSize = ScreenUtils.GetScreenSize();
            _timer = new DispatcherTimer();
            _timer.Tick += TimerTick;
            _defaultPreview = new BitmapImage();

            IsNotRunning = true;
            IsNotFullImage = true;
            Interval = "100";
            Delay = "100";

            StartTakingScreenshots = new RelayCommand(Start);
            StopTakingScreenshots = new RelayCommand(Stop);
            ClearList = new RelayCommand(Clear);
            MoveUpCommand = new RelayCommand<ScreenshotModel>(MoveUp);
            MoveDownCommand = new RelayCommand<ScreenshotModel>(MoveDown);
            CloneCommand = new RelayCommand<ScreenshotModel>(Clone);
            CreateGif = new RelayCommand(MakeGif);
            SelectAreaCommand = new RelayCommand(SelectArea);
            OpenGIFInFolderCommand = new RelayCommand<GifModel>(OpenGIFInFolder);
            CancelGIFCreationCommand = new RelayCommand<GifModel>(CancelGIFCreation);

            Screenshots = new ObservableCollection<ScreenshotModel>();
            Screenshots.CollectionChanged += (_, __) => OnPropertyChanged(nameof(ScreenshotsCount));
            Gifs = new ObservableCollection<GifModel>();

            IsScreenshotMissing = false;
            CanCreateGif = false;

            var dpiXProperty = typeof(Windows.SystemParameters).GetProperty("DpiX", BindingFlags.NonPublic | BindingFlags.Static);
            var dpiYProperty = typeof(Windows.SystemParameters).GetProperty("Dpi", BindingFlags.NonPublic | BindingFlags.Static);
            _originalDpiX = (int)dpiXProperty.GetValue(null, null);
            _originalDpiY = (int)dpiYProperty.GetValue(null, null);

            _folderNames = new List<string>();
        }

        public ICommand StartTakingScreenshots { get; }
        public ICommand StopTakingScreenshots { get; }
        public ICommand ClearList { get; }
        public ICommand CreateGif { get; }
        public ICommand MoveUpCommand { get; }
        public ICommand MoveDownCommand { get; }
        public ICommand SkipCommand { get; }
        public ICommand CloneCommand { get; }
        public ICommand SelectAreaCommand { get; }
        public ICommand OpenGIFInFolderCommand { get; }
        public ICommand CancelGIFCreationCommand { get; }
        public ObservableCollection<ScreenshotModel> Screenshots { get; }
        public GifModel SelectedGif { get; set; }
        public ObservableCollection<GifModel> Gifs { get; }
        public int ScreenshotsCount => Screenshots.Count;
        public bool IsRunning => !IsNotRunning;

        public ScreenshotModel SelectedScreenshot
        {
            get => _selectedScreenshot;
            set
            {
                SetProperty(ref _selectedScreenshot, value);

                if (SelectedScreenshot == null)
                {
                    Preview = _defaultPreview;
                    IsScreenshotMissing = false;
                }
                else
                if (File.Exists(SelectedScreenshot.ImagePath))
                {
                    using var bitmap = new Bitmap(SelectedScreenshot.ImagePath);
                    Preview = bitmap.ToBitmapImage();

                    IsScreenshotMissing = false;
                }
                else
                {
                    IsScreenshotMissing = true;
                }
            }
        }

        public BitmapImage Preview
        {
            get => _preview;
            private set => SetProperty(ref _preview, value);
        }

        public string Interval
        {
            get => _interval.ToString();
            set
            {
                if (int.TryParse(value, out int result) &&
                    result > 0)
                {
                    SetProperty(ref _interval, result);
                }
            }
        }

        public bool IsNotRunning
        {
            get => _isNotRunning;
            set
            {
                SetProperty(ref _isNotRunning, value);
                OnPropertyChanged(nameof(IsRunning));
            }
        }

        public bool CanCreateGif
        {
            get => _canCreateGif;
            private set => SetProperty(ref _canCreateGif, value);
        }

        public string Delay
        {
            get => _delay.ToString();
            set
            {
                if (int.TryParse(value, out int result) &&
                    result > 0)
                {
                    SetProperty(ref _delay, result);
                }
            }
        }

        public bool IsNotFullImage
        {
            get => _isNotFullImage;
            set
            {
                SetProperty(ref _isNotFullImage, value);

                _isFullImage = !_isNotFullImage;
                OnPropertyChanged(nameof(IsFullImage));
            }
        }

        public bool IsFullImage
        {
            get => _isFullImage;
            set
            {
                SetProperty(ref _isFullImage, value);

                _isNotFullImage = !_isFullImage;
                OnPropertyChanged(nameof(IsNotFullImage));
            }
        }

        public bool IsScreenshotMissing
        {
            get => _isScreenshotMissing;
            private set => SetProperty(ref _isScreenshotMissing, value);
        }

        private void TimerTick(object sender, EventArgs e)
        {
            try
            {
                _currentScreenshotNumber += 1;

                using var bitmap = new Bitmap(_screenSize.Width, _screenSize.Height, PixelFormat.Format24bppRgb);
                bitmap.SetResolution(96, 96);

                using var g = Graphics.FromImage(bitmap);
                g.CopyFromScreen(Point.Empty, Point.Empty, _screenSize);
                
                var screenshotName = _currentFolderName + "\\" + _startTime.GetConvinientTimeFormat() + "_" + _currentScreenshotNumber + ".jpg";
                var screenshot = new ScreenshotModel(screenshotName, bitmap);
                bitmap.Save(screenshotName);

                Screenshots.Add(screenshot);

            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
        }

        private void Start()
        {
            _startTime = DateTime.Now;
            _currentScreenshotNumber = 0;
            var folder = Directory.CreateDirectory(_startTime.GetConvinientTimeFormat());
            _currentFolderName = folder.FullName;
            _folderNames.Add(folder.FullName);

            IsNotRunning = false;

            _timer.Interval = new TimeSpan(0, 0, 0, 0, _interval);
            _timer.Start();
        }

        private void Stop()
        {
            IsNotRunning = true;

            _timer.Stop();
        }

        private void Clear()
        {
            Screenshots.Clear();
        }

        private void MoveUp(ScreenshotModel model)
        {
            var index = Screenshots.IndexOf(model);
            if (index - 1 >= 0)
            {
                Screenshots.Move(index, index - 1);
            }
        }

        private void MoveDown(ScreenshotModel model)
        {
            var index = Screenshots.IndexOf(model);
            if (index + 1 <= Screenshots.Count - 1)
            {
                Screenshots.Move(index, index + 1);
            }
        }

        private void Clone(ScreenshotModel model)
        {
            var index = Screenshots.IndexOf(model);
            Screenshots.Insert(index + 1, new ScreenshotModel(model));
        }

        private void SelectArea()
        {
            if (Screenshots.Count == 0)
            {
                return;
            }

            if (_croppingWindow != null)
            {
                return;
            }

            using var croppingToolBitmap = new Bitmap(Screenshots[0].ImagePath);
            croppingToolBitmap.SetResolution(_originalDpiX, _originalDpiY);

            var croppingToolBitmapImage = croppingToolBitmap.ToBitmapImage();

            _croppingWindow = new CroppingWindow(croppingToolBitmapImage);
            _croppingWindow.Closed += (a, b) =>
            {
                _croppingWindow = null;
                CanCreateGif = false;
            };
            _croppingWindow.Width = Screenshots[0].Size.Width;
            _croppingWindow.Height = Screenshots[0].Size.Height;
            
            _croppingWindow.Show();

            CanCreateGif = true;
        }

        private async void MakeGif()
        {
            if (!CanCreateGif)
            {
                return;
            }

            if (Screenshots.Count == 0)
            {
                return;
            }

            if (_croppingWindow == null)
            {
                return;
            }

            var areAllSkipped = true;
            foreach (var screenshot in Screenshots)
            {
                if (!screenshot.IsSkipped)
                {
                    areAllSkipped = false;
                }
            }
            
            if (areAllSkipped)
            {
                return;
            }

            var cropArea = _croppingWindow.CropTool.CropService.GetCroppedArea();
            var x = Convert.ToInt32(cropArea.CroppedRectAbsolute.X * _originalDpiX / 96.0);
            var y = Convert.ToInt32(cropArea.CroppedRectAbsolute.Y * _originalDpiY / 96.0);
            var width = Convert.ToInt32(cropArea.CroppedRectAbsolute.Width * _originalDpiX / 96.0);
            var height = Convert.ToInt32(cropArea.CroppedRectAbsolute.Height * _originalDpiY / 96.0);

            //if there's no selection, select whole area
            if (x == 0 &&
                y == 0 &&
                width == 0 &&
                height == 0)
            {
                width = Convert.ToInt32(Screenshots[0].Size.Width);
                height = Convert.ToInt32(Screenshots[0].Size.Height);
            }

            width = width == 0 ? 1 : width;
            height = height == 0 ? 1 : height;

            var cropRect = new Rectangle(x, y, width, height);

            _croppingWindow.Close();

            Debug.WriteLine("Cropping rectangle: x {2}, y {3}, width {0}, height {1}", cropRect.Width, cropRect.Height, cropRect.X, cropRect.Y);

            var saveFileDialog = new SaveFileDialog();
            saveFileDialog.FileName = "New Gif";
            saveFileDialog.ValidateNames = true;
            saveFileDialog.Filter = ".gif (*.gif)|*.gif";

            if (saveFileDialog.ShowDialog() != true)
            {
                return;
            }

            Debug.WriteLine("File name " + saveFileDialog.FileName);

            var screenshotsPaths = new List<string>();
            foreach (var screenshot in Screenshots)
            {
                if (screenshot.IsSkipped)
                {
                    continue;
                }

                screenshotsPaths.Add(screenshot.ImagePath);
            }

            var gifModel = new GifModel(saveFileDialog.FileName, screenshotsPaths, _delay, Screenshots[0].Size, cropRect);
            Gifs.Add(gifModel);
            await gifModel.CreateGif();
        }

        private void OpenGIFInFolder(GifModel gifModel)
        {
            if (gifModel == null)
            {
                return;
            }

            if (!File.Exists(gifModel.ImagePath))
            {
                System.Windows.MessageBox.Show("File '" + gifModel.Name + "' was removed or deleted.",
                    "File not found",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);

                return;
            }

            string argument = "/select, \"" + gifModel.ImagePath + "\"";
            Process.Start("explorer.exe", argument);
        }

        public void Close()
        {
            if (_croppingWindow != null)
            {
                _croppingWindow.Close();
            }

            for (int i = 0; i < _folderNames.Count; i++)
            {
                try
                {
                    Directory.Delete(_folderNames[i], true);
                }
                catch (Exception)
                {
                }
            }
        }

        public void CancelGIFCreation(GifModel gifModel)
        {
            var result = Windows.MessageBox.Show("Do you want to cancel creation of '" + gifModel.Name + "'?", "Cancel confirm", Windows.MessageBoxButton.YesNo, Windows.MessageBoxImage.Exclamation);
            if (result != Windows.MessageBoxResult.Yes)
            {
                return;
            }

            gifModel.Cancel();
        }
    }
}
