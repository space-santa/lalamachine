using System;
using System.IO;
using System.Runtime.Versioning;

namespace LibLala.Audio.Windows
{
    [SupportedOSPlatform("windows")]
    internal static class MediaFoundation
    {
        // Media Foundation's unit of time is 100 ns.
        public const double TicksPerSecond = 10_000_000;

        // Started once for the life of the app. The HRESULT, false when Windows has no Media Foundation.
        private static readonly Lazy<(int Status, bool IsThere)> Startup = new(StartOnce);

        // Throws an IOException when it can't start, the file can't be read then.
        public static void Start(string path)
        {
            var (status, isThere) = Startup.Value;
            if (!isThere)
            {
                throw new IOException($"Can't read {path}: Windows has no Media Foundation, the N editions need the Media Feature Pack.");
            }
            Check(status, path);
        }

        public static void Check(int status, string path)
        {
            if (status < 0)
            {
                throw new IOException($"Can't read {path}: Windows error 0x{status:X8}.");
            }
        }

        // The time of the sample at the rate, in 100 ns.
        public static long Time(long sample, int sampleRate) => (long)Math.Round(sample * TicksPerSecond / sampleRate);

        private static (int Status, bool IsThere) StartOnce()
        {
            try
            {
                return (NativeMethods.MFStartup(NativeMethods.MediaFoundationVersion, NativeMethods.StartupLite), true);
            }
            catch (DllNotFoundException)
            {
                return (0, false);
            }
        }
    }
}
