using System;
using System.Windows.Data;

namespace Lalamachine.Wpf.View.Converter
{
    [ValueConversion(typeof(double), typeof(string))]
    public class SecondsToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double seconds = (double)value;
            TimeSpan timeSpan = new TimeSpan(0, 0, (int)seconds);
            return timeSpan.ToString(@"m\:ss");
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
