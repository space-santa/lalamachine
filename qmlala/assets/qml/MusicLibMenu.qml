import QtQuick
import QtQuick.Controls

Menu {
    title: "MusicLib"
    signal showSetLibraryDialog
    signal rescanLibrary

    MenuItem {
        text: "Rescan"
        icon.source: "../images/refresh.png"
        onTriggered: {
            rescanLibrary();
        }
    }
    MenuSeparator {}
    MenuItem {
        text: "Set library"
        icon.source: "../images/preferences.png"
        onTriggered: {
            showSetLibraryDialog();
        }
    }
}
