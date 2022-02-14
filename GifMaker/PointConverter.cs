using System;
using System.Windows;
using System.Globalization;
using System.Windows.Data;

namespace GifMaker
{
    public class PointConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var point = (System.Drawing.Point)value;
            return point.X + ", " + point.Y;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return DependencyProperty.UnsetValue;
        }
    }
}
