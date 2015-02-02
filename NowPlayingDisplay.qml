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

    signal seek(var pos)

    Slider {
        id: progress_timer
        height: 50
        anchors.left: parent.left
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
