import QtQuick 2.0



Item {
    signal rightClick
    Rectangle {
        anchors {
            left: parent.left
            right: parent.right
            verticalCenter: parent.verticalCenter
        }
        height: parent.height
        color: styleData.selected ? 'darkslategrey' : "transparent"
        MouseArea {
            anchors.fill: parent
            acceptedButtons: Qt.RightButton
            onClicked: {
                if (mouse.button == Qt.RightButton) {
                    console.log("Right-Click", styleData.row)
                    if (playlist_view.selection.count < 2) {
                        playlist_view.selection.clear()
                        playlist_view.selection.select(styleData.row)
                    }
                }
                rightClick()
            }
        }
    }
}
