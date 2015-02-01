import QtQuick 2.3
import QtQuick.Controls 1.2
import QtMultimedia 5.0
import QtQuick.Dialogs 1.2

import "qrc:/functions.js" as Functions

ApplicationWindow {
    visible: true
    width: 750
    height: 480
    title: qsTr("lalamachine")

    Rectangle {
        anchors.top: parent.top
        anchors.left: parent.left
        anchors.bottom: parent.bottom
        anchors.right: parent.right
        gradient: Gradient {
            GradientStop {
                position: 0.49
                color: "#000000"
            }
            GradientStop {
                position: 1.00
                color: "#717171"
            }
        }
    }

    MediaPlayer {
        id: playMusic
        volume: volume_control.value

        onStopped: {
            if (Functions.millisToSec(position) === Functions.millisToSec(
                        duration)) {
                playlist.playNext()
            }
        }
    }

    Rectangle {
        id: playlist_container
        anchors.left: parent.left
        anchors.right: parent.right
        anchors.top: parent.top
        anchors.bottom: now_playing_container.top
        color: "transparent"

        PlaylistButtons {
            id: playlist_buttons
            anchors.left: parent.left
            anchors.bottom: parent.bottom

            onSaveList: playlist.savePlaylistVisible = true
            onMoveTop: playlist.moveTop()
            onMoveUp: playlist.moveUp()
            onMoveDown: playlist.moveDown()
            onMoveBottom: playlist.moveBottom()
            onClearList: playlist.clearList()
        }

        Playlist {
            id: playlist
            anchors.left: playlist_buttons.right
            anchors.right: parent.right
            anchors.top: parent.top
            anchors.bottom: parent.bottom

            onPlay: {
                playMusic.source = path
                playMusic.play()
            }
        }
    }

    Rectangle {
        id: now_playing_container
        anchors.bottom: btn_row.top
        anchors.right: parent.right
        width: playlist.width
        height: 100
        color: "transparent"
        Slider {
            id: progress_timer
            height: 50
            anchors.left: parent.left
            anchors.bottom: parent.bottom
            anchors.right: parent.right
            anchors.leftMargin: 20
            anchors.rightMargin: 20
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
            color: "#ffffff"
            anchors.right: parent.right
            anchors.rightMargin: 20
            anchors.bottom: progress_timer.top
            verticalAlignment: Text.AlignVCenter
            text: getNowPlayingInfo() + Functions.millisToMinSec(
                      progress_timer.value * playMusic.duration) + " / " + Functions.millisToMinSec(
                      playMusic.duration)
            font.pointSize: 12
            styleColor: "#000000"
            style: Text.Outline

            function getNowPlayingInfo() {
                var retVal = ""

                if (typeof (playMusic.metaData.title) !== "undefined") {
                    retVal += playMusic.metaData.title
                }
                if (typeof (playMusic.metaData.albumArtist) !== "undefined") {
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
    }

    PlayerControlButtons {
        id: btn_row
        anchors.left: parent.left
        anchors.bottom: parent.bottom

        onPlayPrevious: playlist.playPrevious()

        onPlay: {
            if (playMusic.hasAudio) {
                playMusic.play()
            }
        }

        onPause: playMusic.pause()
        onOpen: {
            console.log("open clicked")
            fileDialog.visible = true
        }
        onPlayNext: playlist.playNext()
    }

    VolumeControl {
        id: volume_control
        anchors.bottom: parent.bottom
        anchors.left: btn_row.right
        anchors.right: parent.right
    }

    FileDialog {
        id: fileDialog
        visible: false
        title: "Please choose a file"
        selectMultiple: true
        nameFilters: ["Audio files (*.mp3 *.m4a *.ogg *.wav)"]

        onAccepted: {
            console.log("You chose: " + fileDialog.fileUrls)
            playlist.addList(fileDialog.fileUrls)
            visible = false
        }
        onRejected: {
            console.log("Canceled")
            visible = false
        }
    }
}
