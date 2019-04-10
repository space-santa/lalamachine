import QtQuick.Controls.Styles 1.4
import QtQuick 2.0

SliderStyle {
    groove: Rectangle {
        implicitHeight: 6
        implicitWidth: 500
        color: "transparent"

        Rectangle {
            id: filled
            anchors.left: parent.left
            anchors.top: parent.top
            anchors.bottom: parent.bottom
            color: "orange"
            radius: 6
            width: styleData.handlePosition
            implicitHeight: 6
        }

        Rectangle {
            id: empty
            anchors.left: filled.right
            anchors.top: parent.top
            anchors.bottom: parent.bottom
            anchors.right: parent.right
            color: "black"
            radius: 6
            implicitHeight: 6
            opacity: 0.3
        }
    }
}