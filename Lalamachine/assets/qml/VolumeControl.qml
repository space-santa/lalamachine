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

    onValueChanged: {
        lalaplayer.volume = value
    }

    Component.onCompleted: {
        value = config.volume
    }

    Component.onDestruction: {
        config.volume = value
    }

    function volumeUp() {
        var tmpvol = value
        if (tmpvol < 0.9) {
            tmpvol += 0.1
        } else {
            tmpvol = 1
        }
        value = Math.round(tmpvol * 10) / 10
    }

    function volumeDown() {
        var tmpvol = value
        if (tmpvol > 0.1) {
            tmpvol -= 0.1
        } else {
            tmpvol = 0
        }
        value = Math.round(tmpvol * 10) / 10
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
            source: "../images/lalamachine.png"
            visible: !mute_icon.visible

            function iconSize() {
                // The icon should still be visible if volume is 0.
                return parent.height * (0.25 + 0.75 * volume_slider.value)
            }
        }
        Image {
            id: mute_icon
            anchors.horizontalCenter: parent.horizontalCenter
            anchors.bottom: parent.bottom
            height: vol_icon.height
            width: height
            source: "../images/no.png"
            visible: playMusic.muted
        }

        MouseArea {
            id: muter
            anchors.fill: parent
            onClicked: playMusic.muted = !playMusic.muted
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

        style: SliderStyleLala {
        }
    }
}
