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

    Slider {
        id: volume_slider
        height: 50
        width: 210
        anchors.left: btn_row.right
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

        onPressedChanged: {
            if (!pressed) {
                playMusic.seek(value * playMusic.duration)
            }
        }
    }
    Text {
        id: time_text
        height: 50
        anchors.horizontalCenter: progress_timer.horizontalCenter
        anchors.bottom: progress_timer.top
        verticalAlignment: Text.AlignVCenter
        text: Functions.millisToMinSec(progress_timer.value * playMusic.duration)
              + "/"
              + Functions.millisToMinSec(playMusic.duration)
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
