import QtQuick 2.0
import QtQuick.Controls 1.3

import Lala 1.0

Menu {
    id: rcm
    title: "track"

    property bool isLibrary: false

    signal addToPlaylist(string listname)
    signal deleteSelection

    MenuItem {
        text: "Add to current playlist"
        onTriggered: addToPlaylist(miscPlaylistName)
        visible: isLibrary
    }

    MenuItem {
        text: "Add to new playlist"
        onTriggered: addToPlaylist("")
        visible: isLibrary
    }

    PlaylistMenu {
        id: playlist_menu
        title: "Add to playlist"
        onSelected: addToPlaylist(listname)
    }

    MenuSeparator {
    }

    MenuItem {
        text: "delete"
        visible: !rcm.isLibrary
        onTriggered: deleteSelection()
    }
}
