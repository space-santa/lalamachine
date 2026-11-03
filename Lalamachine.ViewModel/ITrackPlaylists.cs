using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LibLala.LibLalaTagReader;

namespace Lalamachine.ViewModel
{
    public class LikedChangedEventArgs : EventArgs
    {
        public LikedChangedEventArgs(IReadOnlySet<string> paths, bool isLiked)
        {
            Paths = paths;
            IsLiked = isLiked;
        }

        public IReadOnlySet<string> Paths { get; }
        public bool IsLiked { get; }
    }

    // What every list of tracks needs from the named playlists.
    public interface ITrackPlaylists
    {
        // Every list has its own objects for the same track, this keeps all their hearts the same.
        event EventHandler<LikedChangedEventArgs>? LikedChanged;

        bool IsLiked(string path);

        void ToggleLiked(LibLalaTags track);

        TrackPlaylistChoices ChoicesFor(IReadOnlyCollection<LibLalaTags> tracks);

        // Asks for the name.
        Task SaveAsNewPlaylistAsync(IEnumerable<LibLalaTags> tracks);
    }
}
