﻿using System;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace GifMaker
{
    public class SizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var size = (System.Drawing.Size)value;
            return size.Width + ", " + size.Height;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}