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

    signal selected(string listname)

    function populatePlaylistMenu(names) {
        playlist_menu.clear()
        for (var i = 0; i < names.length; ++i) {
            let name = names[i];
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
