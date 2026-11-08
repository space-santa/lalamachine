using System;
using LibLala.MediaControls;
using LibLala.MediaControls.MacOS;

namespace Lalamachine.Desktop.Services
{
    internal static class MediaControlsFactory
    {
        // The buttons of Bluetooth headphones, the media keys and what the system shows as playing. Null where lalamachine
        // doesn't have them.
        public static IMediaControls? Create()
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
    }
}
