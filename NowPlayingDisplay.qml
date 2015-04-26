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

import "qrc:/functions.js" as Functions

Rectangle {
    id: now_playing_container
    height: 100
    color: "transparent"
    property string title
    property string albumArtist
    property int duration
    property int position
    property bool hasAudio
    property bool repeat: repeat_btn.checked
    property alias repeatAll: repeat_btn.repeatAll
    property alias repeatOne: repeat_btn.repeatOne
    property alias random: repeat_btn.random

    signal seek(var pos)

    MultiStateButton {
        id: repeat_btn
        width: 50
        anchors.left: parent.left
        anchors.bottom: parent.bottom
    }

    Slider {
        id: progress_timer
        height: 50
        anchors.left: repeat_btn.right
        anchors.bottom: parent.bottom
        anchors.right: parent.right
        anchors.leftMargin: 20
        anchors.rightMargin: 20
        value: position / duration
        enabled: hasAudio

        onPressedChanged: {
            if (!pressed) {
                seek(value * duration)
            }
        }
    }

    Text {
        id: time_text
        height: 50
        color: "#ffffff"
        anchors.right: parent.right
        anchors.rightMargin: 20
        anchors.left: parent.left
        anchors.leftMargin: 20
        anchors.bottom: progress_timer.top
        verticalAlignment: Text.AlignVCenter
        horizontalAlignment: Text.AlignRight
        text: getNowPlayingInfo() + Functions.millisToMinSec(
                  progress_timer.value * duration) + " / " + Functions.millisToMinSec(
                  duration)
        font.pointSize: 12
        styleColor: "#000000"
        style: Text.Outline

        function getNowPlayingInfo() {
            var retVal = ""

            if (title !== "") {
                retVal += title
            }
            if (typeof albumArtist !== "") {
                if (retVal.length > 0) {
                    retVal += " - "
                }
                retVal += albumArtist
            }
            if (retVal.length > 0) {
                retVal += " | "
            }

            return retVal
        }
    }
}
