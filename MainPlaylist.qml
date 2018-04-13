import QtQuick 2.0

import Lala 1.0

Playlist {
    id: main_playlist

    onShowRightClickMenu: rcm.popup()
    signal addTracksToNamedPlaylist(string listname, var tracks)

    MainPlaylistRightClickMenu {
        id: rcm
        onAddToPlaylist: main_playlist.addTracksToNamedPlaylist(listname, getSelectedTracks())
        onDeleteSelection: main_playlist.deleteSelection()
    }
}
