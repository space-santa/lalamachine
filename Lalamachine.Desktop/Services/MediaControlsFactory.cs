using System;
using LibLala.MediaControls;
using LibLala.MediaControls.MacOS;

namespace Lalamachine.Desktop.Services
{
    internal static class MediaControlsFactory
    {
        // The buttons of Bluetooth headphones, the media keys and what the system shows as playing. Null where lalamachine
        // doesn't have them, or the system can't make them.
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "CA1031:Do not catch general exception types", Justification = "The controls are an extra. Where the system can't make them, like Windows N without its Media Feature Pack, lalamachine plays without.")]
        public static IMediaControls? Create()
        {
            try
            {
                if (OperatingSystem.IsMacOS())
                {
                    return new RemoteCommandCenter();
                }
#if WINDOWS
                return new TransportControls();
#else
                return null;
#endif
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
