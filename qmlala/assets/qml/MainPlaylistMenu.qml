import QtQuick
import QtQuick.Controls

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
        icon.source: "../images/open.png"
        onSelected: openPlaylist(listname)
    }
    MenuItem {
        text: "save"
        action: saveAction
    }
    MenuItem {
        text: "save as"
        icon.source: "../images/save.png"
        onTriggered: showSavePlaylistDialog()
    }

    MenuSeparator {}
    PlaylistMenu {
        id: delete_menu
        title: "Delete"
        icon.source: "../images/delete.png"
        onSelected: deletePlaylist(listname)
    }
    MenuSeparator {}
    MenuItem {
        text: "Export Files"
        icon.source: "../images/export.png"
        onTriggered: openExportDialog()
    }
    MenuSeparator {}
    MenuItem {
        text: "Settings"
        icon.source: "../images/preferences.png"
        onTriggered: openSettingsDialog()
    }
}
