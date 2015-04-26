/*
Copyright 2015 Armin Zirkel

This file is part of lalamachine.

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
*/
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
