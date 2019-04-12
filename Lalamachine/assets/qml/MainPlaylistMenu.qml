import QtQuick 2.0
import QtQuick.Controls 1.2

Menu {
    title: "Playlist"

    property Action saveAction
    signal openPlaylist(string listname)
    signal saveCurrentPlaylist
    signal showSavePlaylistDialog
    signal openSettingsDialog
    signal deletePlaylist(string listname)
    signal openExportDialog

    function updatePlaylistNames(names) {
        playlist_menu.populatePlaylistMenu(names);
        delete_menu.populatePlaylistMenu(names);
    }

    PlaylistMenu {
        id: playlist_menu
        title: "Open"
        iconSource: "../images/open.png"
        onSelected: openPlaylist(listname)
    }
    MenuItem {
        text: "save"
        action: saveAction
    }
    MenuItem {
        text: "save as"
        iconSource: "../images/save.png"
        onTriggered: showSavePlaylistDialog()
    }

    MenuSeparator {
    }
    PlaylistMenu {
        id: delete_menu
        title: "Delete"
        iconSource: "../images/delete.png"
        onSelected: deletePlaylist(listname)
    }
    MenuSeparator {
    }
    MenuItem {
        text: "Export Files"
        iconSource: "../images/export.png"
        onTriggered: openExportDialog()
    }
    MenuSeparator {
    }
    MenuItem {
        text: "Settings"
        iconSource: "../images/preferences.png"
        onTriggered: openSettingsDialog()
    }
}
