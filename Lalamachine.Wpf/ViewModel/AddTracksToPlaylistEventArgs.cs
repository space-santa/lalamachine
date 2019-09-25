using System;
using System.Collections.Generic;
using LibLala.TagReader;

namespace Lalamachine.Wpf.ViewModel
{
    public class AddTracksToPlaylistEventArgs : EventArgs
    {
        public AddTracksToPlaylistEventArgs(List<Tags> tracks, bool newPlaylist)
        {
            Tracks = tracks;
            NewPlaylist = newPlaylist;
        }
        public List<Tags> Tracks { get; }
        public bool NewPlaylist { get; }
    }
}
