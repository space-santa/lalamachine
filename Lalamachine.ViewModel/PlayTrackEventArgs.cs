using System;

namespace Lalamachine.ViewModel
{
    public class PlayTrackEventArgs : EventArgs
    {
        public PlayTrackEventArgs(string path, string listName)
        {
            Path = path;
            ListName = listName;
        }

        public string Path { get; }
        public string ListName { get; }
    }
}
