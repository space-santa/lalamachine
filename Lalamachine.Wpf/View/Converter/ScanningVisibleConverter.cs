using System;
using System.Windows.Data;

namespace Lalamachine.Wpf.View.Converter
{
    /// <summary>
    /// Show XAML Elements when scanning is in prograss.
    /// </summary>
    [ValueConversion(typeof(bool), typeof(string))]
    public class ScanningVisibleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var scanningVisible = (bool)value;
            return scanningVisible ? "Visible" : "Collapsed";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
