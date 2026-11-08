using System;
using System.Runtime.Versioning;
using LibLala.Audio;
using LibLala.Audio.Windows;
using NUnit.Framework;

namespace LibLala.Unittests.Audio
{
    [Category("AudioDevice")]
    [SupportedOSPlatform("windows")]
    public sealed class WaveOutOutputTest : AudioOutputContractTest
    {
        protected override string? Unsupported => OperatingSystem.IsWindows() ? null : "Plays through Windows's waveOut.";

        protected override IAudioOutput Create(IAudioRenderer renderer) => new WaveOutOutput(renderer);
    }
}
