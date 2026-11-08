using System;
using System.Runtime.Versioning;
using LibLala.Audio;
using LibLala.Audio.MacOS;

namespace LibLala.Unittests.Audio
{
    [SupportedOSPlatform("macos")]
    public sealed class ExtAudioFileDecoderTest : AudioDecoderContractTest
    {
        protected override string? Unsupported => OperatingSystem.IsMacOS() ? null : "Decodes with macOS's codecs.";

        protected override IAudioDecoder Open(string path) => new ExtAudioFileDecoder(path);
    }
}
