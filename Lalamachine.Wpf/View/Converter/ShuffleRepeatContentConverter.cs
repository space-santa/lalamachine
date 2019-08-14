using Lalamachine.Wpf.ViewModel;
using System;
using System.Windows.Data;

namespace Lalamachine.Wpf.View.Converter
{
    [ValueConversion(typeof(ShuffleRepeatState), typeof(string))]
    internal class ShuffleRepeatContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            ShuffleRepeatState state = (ShuffleRepeatState)value;

            switch (state)
            {
                case ShuffleRepeatState.RepeatOne:
                    return "🔂";
                case ShuffleRepeatState.RepeatAll:
                    return "🔁";
                case ShuffleRepeatState.Shuffle:
                    return "🔀";
                default:
                    return "🎵";
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return null;
        }
    }
}
