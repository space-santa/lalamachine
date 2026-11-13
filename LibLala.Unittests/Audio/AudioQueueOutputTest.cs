using System;
using System.Runtime.Versioning;
using LibLala.Audio;
using LibLala.Audio.MacOS;
using NUnit.Framework;

namespace LibLala.Unittests.Audio
{
    [Category("AudioDevice")]
    [SupportedOSPlatform("macos")]
    public sealed class AudioQueueOutputTest : AudioOutputContractTest
    {
        protected override string? Unsupported => OperatingSystem.IsMacOS() ? null : "Plays through macOS's AudioQueue.";

        protected override IAudioOutput Create(IAudioRenderer renderer) => new AudioQueueOutput(renderer);
    }
}
