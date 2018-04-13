import QtQuick 2.0
import QtQuick.Controls 1.2

Menu {
    title: "Info"
    signal showInfoDialog

    Menu {
        title: "Help"
        iconSource: "qrc:/images/images/help.png"
        MenuItem {
            text: "Open the lalamachine wiki in your browser."
            onTriggered: Qt.openUrlExternally("https://github.com/space-santa/lalamachine/wiki")
        }
    }

    Menu {
        title: "Report Bug"
        iconSource: "qrc:/images/images/bug.png"
        MenuItem {
            text: "Create a new lalamachine issue in your browser."
            onTriggered: Qt.openUrlExternally("https://github.com/space-santa/lalamachine/issues")
        }
    }

    MenuItem {
        text: "About lalamachine"
        iconSource: "qrc:/images/images/info.png"
        onTriggered: showInfoDialog()
    }
}
