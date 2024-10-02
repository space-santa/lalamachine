import QtQuick
import QtQuick.Controls

Menu {
    title: "Info"
    signal showInfoDialog

    Menu {
        title: "Help"
        icon.source: "../images/help.png"
        MenuItem {
            text: "Open the lalamachine wiki in your browser."
            onTriggered: Qt.openUrlExternally("https://github.com/space-santa/lalamachine/wiki")
        }
    }

    Menu {
        title: "Report Bug"
        icon.source: "../images/bug.png"
        MenuItem {
            text: "Create a new lalamachine issue in your browser."
            onTriggered: Qt.openUrlExternally("https://github.com/space-santa/lalamachine/issues")
        }
    }

    MenuItem {
        text: "About lalamachine"
        icon.source: "../images/info.png"
        onTriggered: showInfoDialog()
    }
}
