import QtQuick 2.0

import Lala 1.0

Playlist {
    id: main_playlist

    onShowRightClickMenu: rcm.popup()

    MainPlaylistRightClickMenu {
        id: rcm
        onAddToPlaylist: addToPlaylist(listname)
        onDeleteSelection: main_playlist.deleteSelection()
    }
}
