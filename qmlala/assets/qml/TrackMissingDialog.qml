import QtQuick
import QtQuick.Controls
import QtQuick.Dialogs

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
            anchors.fill: parent
            verticalAlignment: Qt.AlignVCenter
            id: msg
            wrapMode: Text.WordWrap
            anchors.centerIn: parent
        }
    }
}
