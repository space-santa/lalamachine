using System;

namespace Lalamachine.Wpf.ViewModel
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
