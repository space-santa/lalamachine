using System;

namespace LibLala.MediaControls
{
    public enum MediaCommand
    {
        Play,
        Pause,
        TogglePlayPause,
        Next,
        Previous,
        // To the position that comes with it.
        Seek,
    }

    public sealed class MediaCommandEventArgs(MediaCommand command, TimeSpan position = default) : EventArgs
    {
        public MediaCommand Command { get; } = command;

        // Where to, for Seek.
        public TimeSpan Position { get; } = position;
    }

    // The track as the operating system shows it, like in macOS's Control Center. Artist and album can be empty. The cover is
    // the picture in the track's tags, like a JPEG, null without one: the same array each time for the same track, so the
    // controls can keep what they made of it.
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Performance", "CA1819:Properties should not return arrays", Justification = "The picture as it's stored, it's handed on as it is.")]
    public sealed record NowPlaying(string Title, string Artist, string Album, TimeSpan Duration, TimeSpan Position, bool IsPlaying, byte[]? Cover = null);

    // The operating system's controls for what plays: the buttons of Bluetooth headphones, a keyboard's media keys, macOS's
    // Control Center. The system sends them to the app that it shows as playing, so the app has to say what it plays.
    public interface IMediaControls : IDisposable
    {
        // A button was pressed. On whatever thread the system calls back on.
        event EventHandler<MediaCommandEventArgs>? CommandReceived;

        // From here on the system shows this, and counts on from Position while it plays. Null when nothing is loaded.
        void Show(NowPlaying? nowPlaying);
    }
}
