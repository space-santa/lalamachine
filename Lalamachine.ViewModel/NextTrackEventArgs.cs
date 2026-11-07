using System;

namespace Lalamachine.ViewModel
{
    // Asks the list what it would play next, without playing it or changing anything: for the player to mix it in. The list
    // fills in the track and itself, to be told once it took over.
    public sealed class NextTrackEventArgs : EventArgs
    {
        public PlaylistTags? Track { get; set; }
        public PlaylistBaseViewModel? List { get; set; }
    }
}
