using System;
using System.Runtime.Versioning;
using LalaDb.Model;
using Lalamachine.ViewModel;
using LibLala.Audio;
using LibLala.Audio.MacOS;

namespace Lalamachine.Desktop.Services
{
    internal static class AudioPlayerFactory
    {
        // On macOS lalamachine plays through its own engine, elsewhere through VLC until the engine runs there too.
        // LALAMACHINE_AUDIO=vlc plays through VLC on macOS as well, to compare. LALAMACHINE_CLICK=1 clicks on the beats that
        // the analysis found, to hear whether they're right.
        public static IAudioPlayer Create(TrackAnalysisStore analyses)
        {
            if (OperatingSystem.IsMacOS() && Environment.GetEnvironmentVariable("LALAMACHINE_AUDIO") != "vlc")
            {
                var clicks = Environment.GetEnvironmentVariable("LALAMACHINE_CLICK") == "1";
                return CreateForMac(clicks ? analyses.FindGrid : null);
            }
            return new VlcAudioPlayer();
        }

        // Null where lalamachine can't decode tracks itself yet.
        public static Func<string, IAudioDecoder>? DecoderFactory() => OperatingSystem.IsMacOS() ? MacDecoderFactory() : null;

        [SupportedOSPlatform("macos")]
        private static EngineAudioPlayer CreateForMac(Func<string, BeatGrid?>? findGrid) => new(renderer => new AudioQueueOutput(renderer), MacDecoderFactory(), findGrid);

        [SupportedOSPlatform("macos")]
        private static Func<string, IAudioDecoder> MacDecoderFactory() => path => new ExtAudioFileDecoder(path);
    }
}
