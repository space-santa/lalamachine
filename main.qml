import QtQuick 2.3
import QtQuick.Controls 1.2
import QtMultimedia 5.0
import QtQuick.Dialogs 1.2

import "qrc:/functions.js" as Functions


ApplicationWindow {
    visible: true
    width: 640
    height: 480
    title: qsTr("lalamachine")

    Rectangle {
        anchors.fill: parent
        gradient: Gradient {
            GradientStop {
                position: 0.65;
                color: "#ffbb00";
            }
            GradientStop {
                position: 1.00;
                color: "#ffffff";
            }
        }

    }

    MediaPlayer {
        id: playMusic
        volume: volume_slider.value

        onStopped: {
            console.log("stopped", position, duration)
            if (Functions.millisToSec(position)
                    === Functions.millisToSec(duration)) {
                playlist.playNext()
            }
        }
    }

    Playlist {
        id: playlist
        anchors.left: parent.left
        anchors.right: parent.right
        anchors.top: parent.top
        anchors.bottom: time_text.top

        onPlay: {
            playMusic.source = path
            playMusic.play()
        }
    }

    Row {
        id: btn_row
        anchors.left: parent.left
        anchors.bottom: parent.bottom

        ImageButton {
            id: back_btn
            width: 70
            height: 70
            source: "qrc:/images/images/back.png"

            onClicked: {
                playlist.playPrevious()
            }
        }

        ImageButton {
            id: play_btn
            width: 70
            height: 70
            source: "qrc:/images/images/play.png"

            onClicked: {
                if (playMusic.hasAudio) {
                    playMusic.play()
                }
            }
        }

        ImageButton {
            id: pause_btn
            width: 70
            height: 70
            source: "qrc:/images/images/pause.png"

            onClicked: playMusic.pause()
        }

        ImageButton {
            id: open_btn
            width: 70
            height: 70
            source: "qrc:/images/images/eject.png"

            onClicked: fileDialog.visible = true
        }

        ImageButton {
            id: next_btn
            width: 70
            height: 70
            source: "qrc:/images/images/forward.png"

            onClicked: {
                playlist.playNext()
            }
        }
    }

    Rectangle {
        id: vol_icon_container
        width: 70
        height: width
        anchors.bottom: parent.bottom
        anchors.left: btn_row.right
        color: "transparent"

        Image {
            id: vol_icon
            anchors.horizontalCenter: parent.horizontalCenter
            anchors.bottom: parent.bottom
            height : iconSize()
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
        anchors.bottom: parent.bottom
        value: 0.5
    }

    Slider {
        id: progress_timer
        height: 50
        anchors.left: parent.left
        anchors.bottom: btn_row.top
        anchors.right: parent.right
        value: playMusic.position / playMusic.duration
        enabled: playMusic.hasAudio

        onPressedChanged: {
            if (!pressed) {
                playMusic.seek(value * playMusic.duration)
            }
        }
    }

    Text {
        id: time_text
        height: 50
        anchors.right: parent.right
        anchors.rightMargin: 20
        anchors.bottom: progress_timer.top
        verticalAlignment: Text.AlignVCenter
        text: getMetaData()
              + Functions.millisToMinSec(progress_timer.value * playMusic.duration)
              + " / "
              + Functions.millisToMinSec(playMusic.duration)

        function getMetaData() {
            var retVal = ""

            if (typeof(playMusic.metaData.title) !== "undefined") {
                retVal += playMusic.metaData.title
            }
            if (typeof(playMusic.metaData.albumArtist) !== "undefined") {
                if (retVal.length > 0) {
                    retVal += " - "
                }
                retVal += playMusic.metaData.albumArtist
            }
            if (retVal.length > 0) {
                retVal += " | "
            }

            return retVal
        }
    }

    FileDialog {
        id: fileDialog
        visible: false
        title: "Please choose a file"
        onAccepted: {
            console.log("You chose: " + fileDialog.fileUrls)
            //playMusic.source = fileDialog.fileUrl
            playlist.add(fileDialog.fileUrl.toString())
            visible = false
        }
        onRejected: {
            console.log("Canceled")
            visible = false
        }
    }
}
