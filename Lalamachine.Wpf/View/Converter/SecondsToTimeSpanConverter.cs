using System;
using System.Windows.Data;

namespace Lalamachine.Wpf.View.Converter
{
    [ValueConversion(typeof(double), typeof(string))]
    public class SecondsToTimeSpanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var seconds = value is double ? (double)value : (int)value;
            var timeSpan = new TimeSpan(0, 0, (int)seconds);
            if (timeSpan.TotalDays >= 1)
            {
                return timeSpan.ToString(@"d' days, 'h\:mm\:ss", LibLala.Constants.CULTURE);
            }
            else if (timeSpan.TotalMinutes > 59)
            {
                return timeSpan.ToString(@"h\:mm\:ss", LibLala.Constants.CULTURE);
            }
            else
            {
                return timeSpan.ToString(@"m\:ss", LibLala.Constants.CULTURE);
            }
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
