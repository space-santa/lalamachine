using System;
using System.Windows.Data;
using Lalamachine.Wpf.ViewModel;

namespace Lalamachine.Wpf.View.Converter
{
    [ValueConversion(typeof(ShuffleRepeatState), typeof(string))]
    internal class ShuffleRepeatContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var state = (ShuffleRepeatState)value;

            return state switch
            {
                ShuffleRepeatState.RepeatOne => "🔂",
                ShuffleRepeatState.RepeatAll => "🔁",
                ShuffleRepeatState.Shuffle => "🔀",
                _ => "🎵",
            };
        }

        public object? ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
