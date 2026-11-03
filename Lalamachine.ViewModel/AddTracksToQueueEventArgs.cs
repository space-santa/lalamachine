using System;
using System.Collections.Generic;
using LibLala.LibLalaTagReader;

namespace Lalamachine.ViewModel
{
    public class AddTracksToQueueEventArgs : EventArgs
    {
        public AddTracksToQueueEventArgs(IList<LibLalaTags> tracks, bool replaceQueue)
        {
            Tracks = tracks;
            ReplaceQueue = replaceQueue;
        }
        public IList<LibLalaTags> Tracks { get; }
        public bool ReplaceQueue { get; }
    }
}
