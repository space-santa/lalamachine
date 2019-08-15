using System;
using System.Windows.Data;

namespace Lalamachine.Wpf.View.Converter
{
    [ValueConversion(typeof(bool), typeof(string))]
    public class PlaylistItemIsPlayingConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var isPlaying = (bool)value;
            return isPlaying ? "🔊" : "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
