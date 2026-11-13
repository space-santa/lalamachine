namespace LibLala.Audio
{
    // All audio is in this format between the decoder and the output: interleaved 32-bit float frames, left then right.
    public static class AudioFormat
    {
        public const int SampleRate = 44100;
        public const int Channels = 2;
    }
}
