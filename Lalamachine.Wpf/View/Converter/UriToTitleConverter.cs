using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Data;
using LibLala.TagReader;

namespace Lalamachine.Wpf.View.Converter
{
    [ValueConversion(typeof(string), typeof(string))]
    public class UriToTitleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            string source = (string)value;
            if (source.Length < 1) return "";

            Tags tags = new TagReader().Read(source);
            return $"{tags.artistString} - {tags.title}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
