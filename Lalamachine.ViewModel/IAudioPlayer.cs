using System;

namespace Lalamachine.ViewModel
{
    // Plays the tracks for the PlayerViewModel. All events are raised on the UI thread.
    public interface IAudioPlayer
    {
        event EventHandler? MediaOpened;
        event EventHandler? MediaEnded;
        event EventHandler? MediaFailed;
        // Raised a few times per second so the position slider can follow the track.
        event EventHandler? PositionChanged;

        Uri? Source { get; }
        bool IsMuted { get; set; }
        // From 0 to 1.
        double Volume { get; set; }
        // Null until the track is opened.
        TimeSpan? Duration { get; }
        TimeSpan Position { get; set; }

        void Open(Uri source);
        void Play();
        void Pause();
    }
}
