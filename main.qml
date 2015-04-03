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
    title: getWindowTitle()

    property string miscPlaylistName: "cs1m090"
    property string currentPlaylist: playlist.currentName
    property Playlist mainPlaylist: playlist

    property M3uInOut m3u: m3u

    property MediaPlayer lalaplayer: playMusic

    Component.onCompleted: {
        playlist.currentName = miscPlaylistName
        volume_control.value = config.volume
        console.log("Volume on load", config.volume)
    }

    onClosing: {
        playlist.writePlaylist(miscPlaylistName)
        config.volume = volume_control.value
        config.saveConfig()
    }

    menuBar: MenuBar {
        Menu {
            title: "MusicLib"
            MenuItem {
                text: "Rescan"
                onTriggered: {
                    if (!libview.scanInProgress) {
                        libview.rescan()
                    }
                }
            }
            MenuSeparator {
            }
            MenuItem {
                text: "Set library"
                onTriggered: {
                    lib_dialog.visible = true
                }
            }
        }
        Menu {
            title: "Playlist"
            PlaylistMenu {
                title: "Open"
                iconSource: "qrc:/images/images/open.png"
                onSelected: playlist.currentName = listname
            }
            MenuSeparator {
            }
            PlaylistMenu {
                title: "Delete"
                iconSource: "qrc:/images/images/delete.png"
                onSelected: playlist.deletePlaylist(listname)
            }
            MenuSeparator {
            }
            MenuItem {
                text: "Burn CD"
                iconSource: "file:///usr/share/icons/default.kde4/64x64/apps/k3b.png"
                onTriggered: burnList()
            }
        }
        Menu {
            title: "Info"

            MenuItem {
                text: "Help"
                iconSource: "qrc:/images/images/help.png"
            }

            MenuItem {
                text: "Report Bug"
                iconSource: "qrc:/images/images/bug.png"
            }

            MenuItem {
                text: "About lalamachine"
                iconSource: "qrc:/images/images/info.png"
                onTriggered: infos.visible = true
            }
        }
    }

    InfoDialog {
        id: infos
        visible: false

        onAccepted: visible = false
    }

    function getWindowTitle() {
        var title = ""

        if (playlist.nowPlayingTitle != "") {
            title += playlist.nowPlayingTitle
            title += " | "
        }

        title += "lalamachine"

        return title
    }

    function burnList() {
        playlist.writePlaylist("cs1m090")
        burn.burnList(playlist.getPlaylistPath("cs1m090"))
    }

    M3uInOut {
        id: m3u
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
        onStartPlaying: playlist.playCurrentTrack()
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

        function playTrack(path) {
            // Since the addition of the library it is necessary to
            // make sure an mrl is actually an mrl.
            // FIXME: Find out why!
            playMusic.source = Functions.checkMrl(path)
            playMusic.play()
        }
    }

    Rectangle {
        id: left_shelve
        anchors.left: parent.left
        anchors.top: parent.top
        anchors.bottom: parent.bottom
        width: parent.width / 2
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
                anchors.top: parent.top

                onSaveList: playlist.showSaveDialog()
                onMoveTop: playlist.moveTop()
                onMoveUp: playlist.moveUp()
                onMoveDown: playlist.moveDown()
                onMoveBottom: playlist.moveBottom()
                onClearList: playlist.clearList()
            }

            Playlist {
                id: playlist
                isLibrary: false
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
                    playMusic.playTrack(path)
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
                text: buildText()
                font.pointSize: 12
                styleColor: "#000000"
                style: Text.Outline

                function buildText() {
                    var retval = ""
                    if (playlist.currentName === ""
                            || playlist.currentName === miscPlaylistName) {
                        retval += "new list" + " -- "
                    } else {
                        retval += playlist.currentName + " -- "
                    }

                    retval += playlist.count + " Tracks, Total length = "
                            + playlist.totalPlaytimeString

                    return retval
                }
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
                } else {
                    playlist.playNext()
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
            id: libview
            anchors.fill: parent
            library: config.libPath

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

    FileDialog {
        id: lib_dialog
        visible: false
        title: "Please set your library"
        selectMultiple: false
        selectExisting: true
        selectFolder: true

        onAccepted: {
            config.libPath = lib_dialog.folder
            visible = false
        }
        onRejected: {
            visible = false
        }
    }
}
