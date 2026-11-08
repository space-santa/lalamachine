using System;
using System.Globalization;
using System.IO;
using Avalonia.Data.Converters;
using Avalonia.Media.Imaging;
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
        public static readonly IValueConverter BeatsPerMinuteDisplay = new FuncValueConverter<double, string>(bpm => $"@ {Math.Round(bpm).ToString(CultureInfo.InvariantCulture)} BPM");

        public static readonly IValueConverter AtLeastOne = new FuncValueConverter<double, double>(value => Math.Max(value, 1));

        public static readonly IValueConverter VolumeSize = new FuncValueConverter<double, double>(volume => Math.Floor(16.0 / 100 * volume + 12));

        // The artist too, before the title.
        public static readonly IValueConverter TrackToTitle = new FuncValueConverter<TrackInfo?, string>(track => track switch
        {
            null => "",
            { Artist.Length: > 0 } => $"{track.Artist} - {track.Title}",
            _ => track.Title,
        });

        public static readonly IValueConverter CoverToBitmap = new FuncValueConverter<byte[]?, Bitmap?>(BitmapOf);

        // Twice as wide as Border.cover, sharp on a screen with a high resolution too. Not the thousands of pixels a cover can have.
        private const int CoverWidth = 2 * 136;

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

        // Null when the track has no cover, or one that can't be shown.
        private static Bitmap? BitmapOf(byte[]? cover)
        {
            if (cover is null)
            {
                return null;
            }

            try
            {
                using var stream = new MemoryStream(cover);
                return Bitmap.DecodeToWidth(stream, CoverWidth);
            }
            catch (ArgumentException)
            {
                // Not an image, or one in a format that Avalonia can't read.
                return null;
            }
        }
    }
}
