import QtQuick 2.0
import QtQuick.Controls 1.3
import QtQuick.Dialogs 1.2

Dialog {
    width: 400
    height: 150
    title: "Error"

    onAccepted: text = ""

    standardButtons: StandardButton.Ok

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
