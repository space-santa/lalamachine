import QtQuick 2.3
import QtQuick.Controls 1.2
import QtMultimedia 5.0
import QtQuick.Dialogs 1.2
import QtQuick.Window 2.2

import Lala 1.0

import "qrc:/functions.js" as Functions

ApplicationWindow {
    id: master
    visible: true
    width: Screen.width
    height: Screen.height
    title: qsTr("lalamachine")

    Component.onCompleted: {
        playlist.readPlaylist("cs1m090")
        volume_control.value = config.volume
        console.log("Volume on load", config.volume)
    }

    onClosing: {
        playlist.writePlaylist("cs1m090")
        config.volume = volume_control.value
        config.saveConfig()
    }

    function burnList() {
        playlist.writePlaylist("cs1m090")
        burn.burnList(playlist.getPlaylistPath("cs1m090"))
    }

    KeyEvents {
        id: key_events
        anchors.fill: parent

        onVolumeUp: volume_control.volumeUp()
        onVolumeDown: volume_control.volumeDown()
        onForward: playlist.playNext()
        onBack: playlist.playPrevious()
        onPause: {
            if (playMusic.playbackState === MediaPlayer.PlayingState) {
                playMusic.pause()
                return
            }
            if (playMusic.playbackState === MediaPlayer.PausedState) {
                playMusic.play()
                return
            }
            if (playMusic.playbackState === MediaPlayer.StoppedState) {
                playlist.playCurrentTrack()
                return
            }
        }
        onDeleteTrack: playlist.deleteCurrentTrack()
        onStartPlaying:playlist.playCurrentTrack()
    }

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

    Configuration {
        id: config
    }

    CdWriter {
        id: burn
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
        id: left_shelve
        anchors.left: parent.left
        anchors.top: parent.top
        anchors.bottom: parent.bottom
        width: parent.width/2
        color: "transparent"

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

                onOpenList: playlist.openPlaylistVisible = true
                onSaveList: playlist.savePlaylistVisible = true
                onMoveTop: playlist.moveTop()
                onMoveUp: playlist.moveUp()
                onMoveDown: playlist.moveDown()
                onMoveBottom: playlist.moveBottom()
                onClearList: playlist.clearList()
                onDeleteList: playlist.deletePlaylistVisible = true
                onBurnList: master.burnList()
            }

            Playlist {
                id: playlist
                anchors.left: playlist_buttons.right
                anchors.right: parent.right
                anchors.top: parent.top
                anchors.bottom: playlist_text.top

                repeatAll: now_playing_container.repeat

                nowPlayingSource: playMusic.source

                onStop: {
                    playMusic.stop()
                }

                onPlay: {
                    // Since the addition of the library it is necessary to
                    // make sure an mrl is actually an mrl.
                    // FIXME: Find out why!
                    playMusic.source = Functions.checkMrl(path)
                    playMusic.play()
                }
            }

            Text {
                id: playlist_text
                anchors.left: playlist_buttons.right
                anchors.right: parent.right
                anchors.bottom: parent.bottom
                height: 30
                color: "#ffffff"
                verticalAlignment: Text.AlignVCenter
                horizontalAlignment: Text.AlignLeft
                text: "Total length = " + playlist.totalPlaytimeString
                font.pointSize: 12
                styleColor: "#000000"
                style: Text.Outline
            }
        }

        NowPlayingDisplay {
            id: now_playing_container
            anchors.bottom: btn_row.top
            anchors.right: parent.right
            width: playlist.width

            title: Functions.getSafeValue(playMusic.metaData.title)
            albumArtist: Functions.getSafeValue(playMusic.metaData.albumArtist)
            duration: playMusic.duration
            position: playMusic.position
            hasAudio: playMusic.hasAudio

            onSeek: {
                playMusic.seek(pos)
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
    }

    Rectangle {
        id: right_shelve
        anchors.left: left_shelve.right
        anchors.top: parent.top
        anchors.bottom: parent.bottom
        anchors.right: parent.right
        color: "transparent"

        LibraryView {
            anchors.fill: parent

            onAddTrack: playlist.add(path)
        }
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
