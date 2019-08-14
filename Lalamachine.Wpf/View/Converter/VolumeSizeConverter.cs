using System;
using System.Windows.Data;

namespace Lalamachine.Wpf.View.Converter
{
    [ValueConversion(typeof(double), typeof(int))]
    public class VolumeSizeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double size = (double)value;
            double fraction = 16.0 / 100;
            double displaySize = fraction * size + 12;
            return (int)displaySize;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
