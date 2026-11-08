using System;
using System.Runtime.Versioning;
using LalaDb.Model;
using Lalamachine.ViewModel;
using LibLala.Audio;
using LibLala.Audio.MacOS;
using LibLala.Audio.Windows;

namespace Lalamachine.Desktop.Services
{
    internal static class AudioPlayerFactory
    {
        // On macOS and Windows lalamachine plays through its own engine, elsewhere through VLC until the engine runs there
        // too. Neither app has VLC. LALAMACHINE_CLICK=1 clicks on the beats that the analysis found, to hear whether they're
        // right.
        public static IAudioPlayer Create(TrackAnalysisStore analyses)
        {
            var clicks = Environment.GetEnvironmentVariable("LALAMACHINE_CLICK") == "1";
            if (OperatingSystem.IsMacOS())
            {
                return CreateForMac(analyses.FindGrid, clicks);
            }
            if (OperatingSystem.IsWindows())
            {
                return CreateForWindows(analyses.FindGrid, clicks);
            }
            return new VlcAudioPlayer();
        }

        // Null where lalamachine can't decode tracks itself yet.
        public static Func<string, IAudioDecoder>? DecoderFactory()
        {
            if (OperatingSystem.IsMacOS())
            {
                return MacDecoderFactory();
            }
            if (OperatingSystem.IsWindows())
            {
                return WindowsDecoderFactory();
            }
            return null;
        }

        [SupportedOSPlatform("macos")]
        private static EngineAudioPlayer CreateForMac(Func<string, BeatGrid?> findGrid, bool clicks) => new(renderer => new AudioQueueOutput(renderer), MacDecoderFactory(), findGrid, clicks);

        [SupportedOSPlatform("macos")]
        private static Func<string, IAudioDecoder> MacDecoderFactory() => path => new ExtAudioFileDecoder(path);

        [SupportedOSPlatform("windows")]
        private static EngineAudioPlayer CreateForWindows(Func<string, BeatGrid?> findGrid, bool clicks) => new(renderer => new WaveOutOutput(renderer), WindowsDecoderFactory(), findGrid, clicks);

        [SupportedOSPlatform("windows")]
        private static Func<string, IAudioDecoder> WindowsDecoderFactory() => path => new MediaFoundationDecoder(path);
    }
}
