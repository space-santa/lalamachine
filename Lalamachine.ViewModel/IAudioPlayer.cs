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
        // The track given to MixTo won't take over after all: its transition was called off, by Transitions turned off or a
        // seek back to before it, or it can't be read any more. The one that plays goes on to its end.
        event EventHandler? MixCalledOff;

        // The path of the track's file, null until one is opened.
        string? Source { get; }
        bool IsMuted { get; set; }
        // From 0 to 1.
        double Volume { get; set; }
        // Null until the track is opened.
        TimeSpan? Duration { get; }
        TimeSpan Position { get; set; }
        // Of the track coming in, as heard, from when it does until MixedIn. Null otherwise.
        TimeSpan? IncomingPosition { get; }
        TimeSpan? IncomingDuration { get; }
        // Of the track as heard, of its own: 1 at its own tempo. Else in the bars before a transition on the beat and
        // through it, or after one until it's back at its own.
        double Tempo { get; }
        // Of the track as heard, by its beats at the tempo it plays at. Null when they aren't known.
        double? BeatsPerMinute { get; }
        // Mixes each track into the next, like a DJ. A player that can't, plays them one after the other. Turned off, a
        // transition that didn't start yet is called off.
        bool Transitions { get; set; }

        // The path of a file, as it is: a % in it is a letter of the name, %20 is no space. Not a URI.
        void Open(string path);
        // Mixes the track into the one that plays, after TransitionDue. The source stays the one that plays until MixedIn.
        // False when it can't: the one that plays goes on to its end.
        bool MixTo(string path);
        // A track that can't be read doesn't play, MediaFailed says so again.
        void Play();
        void Pause();
    }
}
