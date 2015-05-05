import QtQuick 2.0
import QtQuick.Dialogs 1.2
import QtQuick.Controls 1.2
import QtQuick.Layouts 1.1

Dialog {
    width: 500
    height: 400

    onAccepted: {
        config.playlistColumns = playlist_columns.getJson()

        config.saveConfig()
    }

    Item {
        id: columns_edit
        height: 200
        width: 220

        KeyValueTableView {
            id: playlist_columns
            anchors.top: parent.top
            anchors.left: parent.left
            anchors.bottom: parent.bottom
            anchors.right: up_button.left
            keyTitle: "Tag"
            valueTitle: "visible"

            list: [{
                    key: "track",
                    value: "true"
                }, {
                    key: "title",
                    value: "true"
                }, {
                    key: "comment",
                    value: "true"
                }, {
                    key: "genre",
                    value: "true"
                }, {
                    key: "artist",
                    value: "true"
                }, {
                    key: "album",
                    value: "true"
                }, {
                    key: "length",
                    value: "true"
                }, {
                    key: "year",
                    value: "true"
                }]

            onRightClick: rcm.popup()
        }

        Menu {
            id: rcm
            MenuItem {
                text: "enable"
                onTriggered: playlist_columns.setSelectionEnabled("true")
            }
            MenuItem {
                text: "disable"
                onTriggered: playlist_columns.setSelectionEnabled("false")
            }
        }

        ImageButton {
            id: up_button
            width: 50
            anchors.right: parent.right
            anchors.bottom: playlist_columns.verticalCenter
            source: "qrc:///images/images/go-up.png"
            onClicked: playlist_columns.moveUp()
        }
        ImageButton {
            id: down_button
            width: 50
            anchors.right: parent.right
            anchors.top: playlist_columns.verticalCenter
            source: "qrc:///images/images/go-down.png"
            onClicked: playlist_columns.moveDown()
        }
    }
}
