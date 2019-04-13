import QtQuick 2.0
import QtQuick.Controls 1.2

Menu {
    title: "MusicLib"
    signal showSetLibraryDialog
    signal rescanLibrary

    MenuItem {
        text: "Rescan"
        iconSource: "../images/refresh.png"
        onTriggered: {
            rescanLibrary()
        }
    }
    MenuSeparator {
    }
    MenuItem {
        text: "Set library"
        iconSource: "../images/preferences.png"
        onTriggered: {
            showSetLibraryDialog()
        }
    }
}
