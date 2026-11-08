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
    }

    public sealed class MediaCommandEventArgs(MediaCommand command) : EventArgs
    {
        public MediaCommand Command { get; } = command;
    }

    // The track as the operating system shows it, like in macOS's Control Center. Artist and album can be empty.
    public sealed record NowPlaying(string Title, string Artist, string Album, TimeSpan Duration, TimeSpan Position, bool IsPlaying);

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
