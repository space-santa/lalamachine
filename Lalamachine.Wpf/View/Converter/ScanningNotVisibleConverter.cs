using System;
using System.Windows.Data;

namespace Lalamachine.Wpf.View.Converter
{
    /// <summary>
    /// Hide XAML elements when scanning is in progress.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class ScanningNotVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var scanningVisible = (bool)value;
            return scanningVisible ? "Collapsed" : "Visible";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
