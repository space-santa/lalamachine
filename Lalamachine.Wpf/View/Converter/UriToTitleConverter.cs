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

            // WPF doesn't catch exceptions thrown by converters, an unreadable file would take the whole app down.
            try
            {
                var tags = new TagReader().Read(source);
                return $"{tags.ArtistString} - {tags.Title}";
            }
            catch (TagReaderException)
            {
                return System.IO.Path.GetFileNameWithoutExtension(source);
            }
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
