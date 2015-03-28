import QtQuick 2.0
import QtQuick.Controls 1.3

import Lala 1.0

Menu {
    id: playlist_menu

    property var playlistnames: m3u.playlistNames

    signal selected(string listname)

    onPlaylistnamesChanged: populatePlaylistMenu()

    Component.onCompleted: {
        populatePlaylistMenu()
    }

    function populatePlaylistMenu() {
        playlist_menu.clear()
        playlistnames.forEach(function (name) {
            var o = Qt.createQmlObject(generateQbjectString(name),
                                       playlist_menu, 'PlaylistMenu')
            playlist_menu.insertItem(playlist_menu.items.length, o)
        })
    }

    // This creates the string for a MenuItem with text 'name' that will call
    // the function 'trigger' with argument 'name'
    function generateQbjectString(name, trigger) {
        var msg = "import QtQuick 2.4\n"
        msg += "import QtQuick.Controls 1.3\n"
        msg += "MenuItem {text: '"
        msg += name
        msg += "';\n onTriggered: selected(text);}"

        return msg
    }
}
