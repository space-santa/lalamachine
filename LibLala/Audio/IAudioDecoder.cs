using System;

namespace LibLala.Audio
{
    // Reads a track as frames of AudioFormat. Throws an IOException when the file can't be read.
    public interface IAudioDecoder : IDisposable
    {
        // In frames.
        long Length { get; }

        // Fills the buffer from the current position on and returns the number of frames. Fewer than fit only at the end.
        int Read(Span<float> buffer);

        void Seek(long frame);
    }
}
