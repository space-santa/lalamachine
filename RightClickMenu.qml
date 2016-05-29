/*
Copyright 2015 Armin Zirkel

This file is part of lalamachine.

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
*/
import QtQuick 2.0
import QtQuick.Controls 1.2

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

    MenuSeparator {
    }

    MenuItem {
        visible: !rcm.isLibrary
        action: new_list_action
    }
}
