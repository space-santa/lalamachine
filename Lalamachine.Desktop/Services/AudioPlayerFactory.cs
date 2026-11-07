using System;
using System.Runtime.Versioning;
using Lalamachine.ViewModel;
using LibLala.Audio.MacOS;

namespace Lalamachine.Desktop.Services
{
    internal static class AudioPlayerFactory
    {
        // On macOS lalamachine plays through its own engine, elsewhere through VLC until the engine runs there too.
        // LALAMACHINE_AUDIO=vlc plays through VLC on macOS as well, to compare.
        public static IAudioPlayer Create()
        {
            if (OperatingSystem.IsMacOS() && Environment.GetEnvironmentVariable("LALAMACHINE_AUDIO") != "vlc")
            {
                return CreateForMac();
            }
            return new VlcAudioPlayer();
        }

        [SupportedOSPlatform("macos")]
        private static EngineAudioPlayer CreateForMac() => new(renderer => new AudioQueueOutput(renderer), path => new ExtAudioFileDecoder(path));
    }
}
