import QtQuick 2.0
import QtQuick.Controls 1.2

Menu {
    title: "Automatic Playlists"

    property var autoPlaylistNames

    signal openAutoPlaylist(string listname)
    signal editAutoPlaylist(string listname)
    signal createNewAutoPlaylist
    signal deleteAutoPlaylist(string listname)

    PlaylistMenu {
        title: "Open"
        iconSource: "qrc:/images/images/open.png"
        playlistnames: autoPlaylistNames
        onSelected: openAutoPlaylist(listname)
    }
    PlaylistMenu {
        title: "Edit"
        iconSource: "qrc:/images/images/edit.png"
        playlistnames: autoPlaylistNames
        onSelected: editAutoPlaylist(listname)
    }
    MenuItem {
        text: "Create"
        iconSource: "qrc:/images/images/new.png"
        onTriggered: createNewAutoPlaylist()
    }
    PlaylistMenu {
        title: "Delete"
        iconSource: "qrc:/images/images/delete.png"
        playlistnames: autoPlaylistNames
        onSelected: deleteAutoPlaylist(listname)
    }
}
