using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibLala.Jellyfin
{
    // Everything throws a JellyfinException when the server can't be reached or refuses, with a message for the user.
    public interface IJellyfinClient
    {
        Task<IReadOnlyList<JellyfinUser>> GetUsersAsync(CancellationToken cancellationToken = default);

        // All the tracks this user can see. Also throws when the server doesn't say where its files are.
        Task<JellyfinAudioLibrary> GetAudioLibraryAsync(string userId, CancellationToken cancellationToken = default);

        // The ones this user can see, that includes other users' public and shared ones.
        Task<IReadOnlyList<JellyfinPlaylist>> GetPlaylistsAsync(string userId, CancellationToken cancellationToken = default);

        // In the order of the playlist.
        Task<IReadOnlyList<JellyfinPlaylistEntry>> GetPlaylistEntriesAsync(string playlistId, string userId, CancellationToken cancellationToken = default);

        // Returns the ID of the new playlist. It belongs to the user, and nobody else can see it.
        Task<string> CreatePlaylistAsync(string name, string userId, IReadOnlyList<string> itemIds, CancellationToken cancellationToken = default);

        // To the end, in the given order. Checks that the user may change the playlist.
        Task AddToPlaylistAsync(string playlistId, IReadOnlyList<string> itemIds, string userId, CancellationToken cancellationToken = default);

        // Doesn't check whose playlist it is, Jellyfin lets an API key remove from any.
        Task RemoveFromPlaylistAsync(string playlistId, IReadOnlyList<string> entryIds, CancellationToken cancellationToken = default);
    }
}
