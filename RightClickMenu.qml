import QtQuick 2.0
import QtQuick.Controls 1.3

Menu {
    id: rcm
    title: "track"

    property var playlistnames: []

    signal addToPlaylist(string listname)

    onAddToPlaylist: {
        console.log("ZZZ", listname)
    }

    onPopupVisibleChanged: {
        populatePlaylistMenu()
    }

    Component.onCompleted: {
        populatePlaylistMenu()
    }

    function populatePlaylistMenu() {
        playlist_menu.clear()
        playlistnames.forEach( function(name) {
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
        msg += String(name)
        msg += "';\n onTriggered: " + trigger + "("
        msg += "text"
        msg += ");}"

        return msg
    }

    MenuItem {
        text: "Add to current playlist"

        onTriggered: addToPlaylist(miscPlaylistName)
    }

    Menu {
        id: playlist_menu
        title: "Add to playlist"
    }
}

