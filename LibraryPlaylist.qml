import QtQuick 2.0

Playlist {
    id: library_playlist
    onShowRightClickMenu: rcm.popup()
    signal addTracksToNamedPlaylist(string listname, var tracks)

    LibraryPlaylistRightClickMenu {
        id: rcm
        onAddToPlaylist: library_playlist.addToPlaylist(listname)
        onAddToNamedPlaylist: library_playlist.addTracksToNamedPlaylist(
                                  listname, getSelectedTracks())
    }
}
