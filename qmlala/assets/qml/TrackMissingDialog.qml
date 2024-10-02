import QtQuick
import QtQuick.Controls

Dialog {
    width: 400
    height: 150
    title: "Error"

    onAccepted: text = ""

    standardButtons: Dialog.Ok

    property alias text: msg.text

    Item {
        implicitWidth: 400
        implicitHeight: 100
        Text {
            id: msg
            anchors.fill: parent
            verticalAlignment: Qt.AlignVCenter
            wrapMode: Text.WordWrap
            anchors.centerIn: parent
        }
    }
}
