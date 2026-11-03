using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace LibLala.Jellyfin
{
    public interface IJellyfinClient
    {
        // Throws a JellyfinException when the server can't be reached or doesn't accept the API key.
        Task<IReadOnlyList<JellyfinUser>> GetUsersAsync(CancellationToken cancellationToken = default);
    }
}
