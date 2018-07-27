import QtQuick 2.0
import QtQuick.Controls 1.2

Menu {
    title: "MusicLib"
    signal showSetLibraryDialog
    signal rescanLibrary

    MenuItem {
        text: "Rescan"
        iconSource: "qrc:/images/images/refresh.png"
        onTriggered: {
            rescanLibrary()
        }
    }
    MenuSeparator {
    }
    MenuItem {
        text: "Set library"
        iconSource: "qrc:/images/images/preferences.png"
        onTriggered: {
            showSetLibraryDialog()
        }
    }
}
