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
    id: playlist_menu

    property var playlistnames: m3u.playlistNames

    signal selected(string listname)

    onPlaylistnamesChanged: decoupler.restart()
    // This timer is necessary to make the current model not segfault.
    // the problem is that a trigger of a menu item in the delete menu will
    // cause the playlist to disappear, which will trigger the menu rebuild
    // because the playlist names changed. The problem is that at that time the
    // signal chain still carries the original emitting object (the menu item)
    // as the sender, because signals always have sender information.
    // But since the first thing that happens is to destroy all menu items to
    // rebuild the menu, the original sender no longer exists.
    // Now the segfault: At some point the sender is dereferenced,
    // but is has already been cleared. Introducing a timer to decouple the
    // original sender from the receiver.
    // And yes, this should be considered a hack.
    Timer {
        id: decoupler
        interval: 100
        repeat: false
        running: false
        onTriggered: populatePlaylistMenu()
    }

    Component.onCompleted: {
        populatePlaylistMenu()
    }

    function populatePlaylistMenu() {
        playlist_menu.clear()
        for (var i = 0; i < playlistnames.length; ++i) {
            let name = playlistnames[i];
            var o = Qt.createQmlObject(generateQbjectString(name),
                                       playlist_menu, 'PlaylistMenu');
            playlist_menu.insertItem(playlist_menu.items.length, o);
        }
    }

    // This creates the string for a MenuItem with text 'name' that will call
    // the function 'trigger' with argument 'name'
    function generateQbjectString(name) {
        var msg = "import QtQuick 2.0\n"
        msg += "import QtQuick.Controls 1.2\n"
        msg += "MenuItem {text: '"
        msg += name
        msg += "';\n onTriggered: selected(text);}"

        return msg
    }
}
