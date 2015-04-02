import QtQuick 2.0

Rectangle {
    width: 200
    height: 40

    color: "transparent"

    property int labelWidth: 60
    property alias labelText: label.text

    property alias valueText: value.text

    Text {
        id: label
        width: labelWidth
        anchors.left: parent.left
        anchors.top: parent.top
        anchors.bottom: parent.bottom

        verticalAlignment: Qt.AlignVCenter
        horizontalAlignment: Qt.AlignRight
    }

    Text {
        id: value
        anchors.right: parent.right
        anchors.top: parent.top
        anchors.bottom: parent.bottom
        anchors.left: label.right

        verticalAlignment: Qt.AlignVCenter
        horizontalAlignment: Qt.AlignLeft
    }
}
