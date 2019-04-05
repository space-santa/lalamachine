// This file is part of lalamachine. License is GPL-3.0 (or later).

import QtQuick 2.0
import QtQuick.Controls 1.2

import Lala 1.0

Menu {
    id: rcm
    title: "track"

    signal addToPlaylist(string listname)
    signal deleteSelection

    PlaylistMenu {
        id: playlist_menu
        title: "Add to playlist"
        onSelected: addToPlaylist(listname)
    }

    MenuItem {
        text: "delete"
        onTriggered: deleteSelection()
    }

    MenuItem {
        action: new_list_action
    }
}
