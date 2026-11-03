using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibLala.LibLalaTagReader;

namespace Lalamachine.ViewModel
{
    public class LikedChangedEventArgs : EventArgs
    {
        public LikedChangedEventArgs(string path, bool isLiked)
        {
            Path = path;
            IsLiked = isLiked;
        }

        public string Path { get; }
        public bool IsLiked { get; }
    }

    // What every list of tracks needs from the named playlists.
    public interface ITrackPlaylists
    {
        // Every list has its own objects for the same track, this keeps all their hearts the same.
        event EventHandler<LikedChangedEventArgs>? LikedChanged;

        bool IsLiked(string path);

        void ToggleLiked(LibLalaTags track);

        TrackPlaylistChoices ChoicesFor(LibLalaTags track);

        // Asks for the name.
        Task SaveAsNewPlaylistAsync(IEnumerable<LibLalaTags> tracks);
    }
}
