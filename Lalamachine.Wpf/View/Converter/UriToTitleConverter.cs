using System;
using System.Windows.Data;
using LibLala.LibLalaTagReader;

namespace Lalamachine.Wpf.View.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class UriToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is null)
            {
                return "";
            }

            var source = (string)value;
            if (source.Length < 1)
            {
                return "";
            }

            var tags = new TagReader().Read(source);
            return $"{tags.ArtistString} - {tags.Title}";
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
