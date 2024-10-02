import QtQuick 2.0

Rectangle {
    height: 30

    TextBase {
        anchors.fill: parent
        text: styleData.value
        verticalAlignment: Qt.AlignVCenter
        horizontalAlignment: Qt.AlignHCenter
    }
}
