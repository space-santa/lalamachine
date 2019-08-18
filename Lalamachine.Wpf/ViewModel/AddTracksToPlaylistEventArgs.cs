using System;
using System.Collections.Generic;
using LibLala.TagReader;

namespace Lalamachine.Wpf.ViewModel
{
    public class AddTracksToPlaylistEventArgs : EventArgs
    {
        public List<Tags> Tracks { get; set; }
        public bool NewPlaylist { get; set; }
    }
}
