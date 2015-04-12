import QtQuick 2.0
import QtQuick.Dialogs 1.2
import QtQuick.Layouts 1.1
import QtQuick.Controls 1.2

Dialog {
    id: container
    width: 600
    height: 300
    standardButtons: StandardButton.Ok | StandardButton.Cancel

    ColumnLayout {
        anchors.fill: parent
        AutoPlaylistObject {
            width: parent.width
            height: 50
        }

        Item {
            id: buttons1
            height: 50
            width: parent.width
            ImageButton {
                id: add_button1
                anchors.left: parent.left
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                width: 50
                source: "qrc:/images/images/list-add.png"

                onClicked: {
                    apo2.visible = true
                    buttons2.visible = true
                    buttons1.visible = false
                }
            }
        }

        AutoPlaylistObject {
            id: apo2
            width: parent.width
            height: 50

            visible: false
        }

        Item {
            id: buttons2
            height: 50
            width: parent.width
            visible: false
            ImageButton {
                id: add_button2
                anchors.left: parent.left
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                width: 50
                source: "qrc:/images/images/list-add.png"

                onClicked: {
                    apo3.visible = true
                    buttons3.visible = true
                    buttons2.visible = false
                }
            }
            ImageButton {
                id: del_button2
                anchors.left: add_button2.right
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                width: 50
                source: "qrc:/images/images/list-remove.png"

                onClicked: {
                    buttons1.visible = true
                    apo2.visible = false
                    buttons2.visible = false
                }
            }
        }

        AutoPlaylistObject {
            id: apo3
            width: parent.width
            height: 50

            visible: false
        }

        Item {
            id: buttons3
            height: 50
            width: parent.width
            visible: false
            ImageButton {
                id: del_button3
                anchors.left: parent.left
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                width: 50
                source: "qrc:/images/images/list-remove.png"

                onClicked: {
                    apo3.visible = false
                    buttons3.visible = false
                    buttons2.visible = true
                }
            }
        }
    }
}
