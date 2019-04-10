// This file is part of lalamachine. License is GPL-3.0 (or later).

import QtQuick 2.0
import QtQuick.Controls 1.2

import Lala 1.0

Menu {
    id: rcm
    title: "track"

    signal addToPlaylist(string listname)
    signal addToNamedPlaylist(string listname)

    MenuItem {
        text: "Add to current playlist"
        onTriggered: addToPlaylist(miscPlaylistName)
    }

    MenuItem {
        text: "Add to new playlist"
        onTriggered: addToPlaylist("")
    }

    PlaylistMenu {
        id: playlist_menu
        title: "Add to playlist"
        onSelected: addToNamedPlaylist(listname)
    }
}
