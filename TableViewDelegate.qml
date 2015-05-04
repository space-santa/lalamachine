import QtQuick 2.0
import QtQuick.Controls 1.2

Item {
    property TableView target
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
                    if (target.selection.count < 2) {
                        target.selection.clear()
                        target.selection.select(styleData.row)
                    }
                }
                rightClick()
            }
        }
    }
}
