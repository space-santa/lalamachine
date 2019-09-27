using System;
using System.Collections.Generic;
using LibLala.LibLalaTagReader;

namespace Lalamachine.Wpf.ViewModel
{
    public class AddTracksToPlaylistEventArgs : EventArgs
    {
        public AddTracksToPlaylistEventArgs(List<LibLalaTags> tracks, bool newPlaylist)
        {
            Tracks = tracks;
            NewPlaylist = newPlaylist;
        }
        public List<LibLalaTags> Tracks { get; }
        public bool NewPlaylist { get; }
    }
}
