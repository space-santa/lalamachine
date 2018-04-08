import QtQuick 2.0

import Lala 1.0

Playlist {
    onShowRightClickMenu: rcm.popup()

    MainPlaylistRightClickMenu {
        id: rcm
        onAddToPlaylist: addToPlaylist(listname)
        onDeleteSelection: deleteCurrentTrack()
    }
}
