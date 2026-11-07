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
        // With Transitions on: the track gets close to where the next one would be mixed in. Time for MixTo.
        event EventHandler? TransitionDue;
        // The track given to MixTo took over, the one before is gone: it's the source from now on.
        event EventHandler? MixedIn;

        Uri? Source { get; }
        bool IsMuted { get; set; }
        // From 0 to 1.
        double Volume { get; set; }
        // Null until the track is opened.
        TimeSpan? Duration { get; }
        TimeSpan Position { get; set; }
        // Of the track coming in, as heard, from when it does until MixedIn. Null otherwise.
        TimeSpan? IncomingPosition { get; }
        TimeSpan? IncomingDuration { get; }
        // Mixes each track into the next, like a DJ. A player that can't, plays them one after the other.
        bool Transitions { get; set; }

        void Open(Uri source);
        // Mixes the track into the one that plays, after TransitionDue. The source stays the one that plays until MixedIn.
        void MixTo(Uri source);
        void Play();
        void Pause();
    }
}
