using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Lalamachine.ViewModel;
using LibLala.LibLalaTagReader;

namespace Lalamachine.Desktop
{
    internal static class Converters
    {
        public static readonly IValueConverter IsPlayingDisplay = new FuncValueConverter<bool, string>(isPlaying => isPlaying ? "⏸" : "▶");

        public static readonly IValueConverter MutedDisplay = new FuncValueConverter<bool, string>(muted => muted ? "🔇" : "🔊");

        public static readonly IValueConverter PlaylistItemIsPlaying = new FuncValueConverter<bool, string>(isPlaying => isPlaying ? "🔊" : "");

        public static readonly IValueConverter ShuffleRepeatContent = new FuncValueConverter<ShuffleRepeatState, string>(state => state switch
        {
            ShuffleRepeatState.RepeatOne => "🔂",
            ShuffleRepeatState.RepeatAll => "🔁",
            ShuffleRepeatState.Shuffle => "🔀",
            _ => "🎵",
        });

        // Both the double seconds of the player and the int seconds of the playlists.
        public static readonly IValueConverter SecondsToTime = new FuncValueConverter<object?, string>(value => FormatSeconds(Convert.ToDouble(value, CultureInfo.InvariantCulture)));

        // With a maximum of 0, i.e. before a track is loaded, a slider shows its thumb at the end.
        public static readonly IValueConverter AtLeastOne = new FuncValueConverter<double, double>(value => Math.Max(value, 1));

        public static readonly IValueConverter VolumeSize = new FuncValueConverter<double, double>(volume => Math.Floor(16.0 / 100 * volume + 12));

        public static readonly IValueConverter SourceToTitle = new FuncValueConverter<string?, string>(TitleOf);

        private static string FormatSeconds(double seconds)
        {
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

        private static string TitleOf(string? source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return "";
            }

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
    }
}
