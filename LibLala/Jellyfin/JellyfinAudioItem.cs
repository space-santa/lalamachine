using System.Collections.Generic;

namespace LibLala.Jellyfin
{
    public sealed record JellyfinAudioItem(string Id, string Path);

    // ReportedTotal is what the server says it has. It is more than Items when some tracks came without a path, or when
    // the library changed while it was being read.
    public sealed record JellyfinAudioLibrary(IReadOnlyList<JellyfinAudioItem> Items, int ReportedTotal);
}
