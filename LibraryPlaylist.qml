import QtQuick 2.0

Playlist {
    onShowRightClickMenu: rcm.popup()

    LibraryPlaylistRightClickMenu {
        id: rcm
        onAddToPlaylist: addToPlaylist(listname)
    }
}
