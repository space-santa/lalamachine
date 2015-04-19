import QtQuick 2.0
import QtQuick.Dialogs 1.2
import QtQuick.Layouts 1.1
import QtQuick.Controls 1.2

Dialog {
    id: container
    width: 600
    height: 300
    standardButtons: StandardButton.Ok | StandardButton.Cancel

    property var values
    property alias apName: name_field.text

    function collectValues() {
        var retval = []
        retval[0] = {
            andor: apo1.andorText,
            tag: apo1.whereText,
            operator: apo1.howText,
            value: apo1.valueText
        }

        if (apo2.visible) {
            retval[1] = {
                andor: apo2.andorText,
                tag: apo2.whereText,
                operator: apo2.howText,
                value: apo2.valueText
            }
        }

        if (apo3.visible) {
            retval[2] = {
                andor: apo3.andorText,
                tag: apo3.whereText,
                operator: apo3.howText,
                value: apo3.valueText
            }
        }

        return retval
    }

    function setApo1(andor, where, how, value) {
        apo1.andorText = andor
        apo1.whereText = where
        apo1.howText = how
        apo1.valueText = value
    }
    function setApo2(andor, where, how, value) {
        apo2.andorText = andor
        apo2.whereText = where
        apo2.howText = how
        apo2.valueText = value
    }
    function setApo3(andor, where, how, value) {
        apo3.andorText = andor
        apo3.whereText = where
        apo3.howText = how
        apo3.valueText = value
    }

    function clearAll() {
        apo1.init()
        apo2.init()
        apo3.init()
    }

    function canQuit() {
        return (name_field.text !== "" && apo1.valueText !== "")
    }

    onRejected: {
        clearAll()
        close()
    }

    ColumnLayout {
        anchors.fill: parent

        Layout.fillWidth: true

        Layout.maximumWidth: width

        TextField {
            id: name_field

            width: parent.width
            height: 50

            placeholderText: "playlist name"
        }

        AutoPlaylistObject {
            id: apo1
            width: parent.width
            height: 50

            onVisibleChanged: {
                if (!visible) {
                    apo1.init()
                }
            }
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

            onVisibleChanged: {
                if (!visible) {
                    apo2.init()
                }
            }
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

            onVisibleChanged: {
                if (!visible) {
                    apo3.init()
                }
            }
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
