using System;

namespace LibLala.Audio
{
    // Plays what its IAudioRenderer renders. Play, Pause and Reset are called from one thread, and never while that thread
    // holds a lock that the renderer takes: the output calls the renderer on its own thread and Reset waits for it.
    public interface IAudioOutput : IDisposable
    {
        // The gain, from 0 to 1.
        float Volume { get; set; }

        // Starts, or continues after Pause. Throws an IOException when there's no way to play.
        void Play();

        // Stops right away and keeps what was rendered, Play continues with it. Lets the device go after a while.
        void Pause();

        // Drops what was rendered but not played yet, right away. Render isn't called again until Play. Doesn't pause: after
        // a Reset while playing, Play continues at once, without starting the device again.
        void Reset();
    }

    // Called on the output's thread, and by Play on the thread that calls it.
    public interface IAudioRenderer
    {
        // Fills the whole buffer with frames of AudioFormat.
        void Render(Span<float> buffer);

        // The output played what the oldest Render it didn't report yet rendered. Not called for what Reset dropped.
        void Played();
    }
}
