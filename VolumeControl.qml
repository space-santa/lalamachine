import QtQuick 2.0
import QtQuick.Controls 1.2

Rectangle {
    property alias value: volume_slider.value
    color: "transparent"

    function volumeUp() {
        if (volume_slider.value < 0.9) {
            volume_slider.value += 0.1
        } else {
            volume_slider.value = 1
        }
    }

    function volumeDown() {
        if (volume_slider.value > 0.1) {
            volume_slider.value -= 0.1
        } else {
            volume_slider.value = 0
        }
    }

    Rectangle {
        id: vol_icon_container
        width: 70
        height: width
        anchors.bottom: parent.bottom
        anchors.left: parent.left
        color: "transparent"

        Image {
            id: vol_icon
            anchors.horizontalCenter: parent.horizontalCenter
            anchors.bottom: parent.bottom
            height: iconSize()
            width: height
            source: "qrc:/images/images/lalamachine.png"

            function iconSize() {
                // The icon should still be visible if volume is 0.
                return parent.height * (0.25 + 0.75 * volume_slider.value)
            }
        }
    }

    Slider {
        id: volume_slider
        height: 50
        anchors.left: vol_icon_container.right
        anchors.right: parent.right
        anchors.rightMargin: 20
        anchors.bottom: parent.bottom
        value: 0.5
    }
}
