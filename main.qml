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
import Qt.labs.settings 1.0

import Lala 1.0

import "qrc:/functions.js" as Functions

ApplicationWindow {
    id: master
    visible: true

    width: Screen.width / 2
    height: Screen.height / 2
    title: getWindowTitle()

    minimumHeight: 500
    minimumWidth: 1005

    property alias musicLib: libview.musicLib
    property string miscPlaylistName: "cs1m090"
    property string currentPlaylist: playlist.currentName
    property Playlist mainPlaylist: playlist
    property M3uInOut m3u: m3u
    property Configuration config: config

    property MediaPlayer lalaplayer: playMusic

    Component.onCompleted: {
        playlist.setCurrentPlaylist(config.lastPlaylist)
        volume_control.value = config.volume
        console.log("Volume on load", config.volume)
    }

    Settings {
        property alias x: master.x
        property alias y: master.y
        property alias width: master.width
        property alias height: master.height
    }

    onClosing: {
        playlist.writePlaylist(miscPlaylistName)
        config.volume = volume_control.value

        // If the current list has a name we want to remember latest changes.
        playlist.writeCurrentListIfNamed()

        // I am deliberatly not checking for a named playlist here.
        // If the currentPlaylist is not empty but the miscPlaylistName it still
        // as expected because then the lastPlaylist is set to the
        // miscPlaylistName in the else branch.
        if (currentPlaylist === "") {
            config.lastPlaylist = miscPlaylistName
        } else {
            config.lastPlaylist = currentPlaylist
        }
        config.saveConfig()
    }

    menuBar: MenuBar {
        Menu {
            title: "MusicLib"
            MenuItem {
                text: "Rescan"
                iconSource: "qrc:/images/images/refresh.png"
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
                iconSource: "qrc:/images/images/preferences.png"
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
                onSelected: playlist.setCurrentPlaylist(listname)
            }
            MenuItem {
                text: "save as"
                iconSource: "qrc:/images/images/save.png"
                onTriggered: playlist.showSaveDialog()
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
                iconSource: "qrc:/images/images/burn.png"
                onTriggered: burnList()
            }
            MenuItem {
                text: "Export Files"
                iconSource: "qrc:/images/images/export.png"
                onTriggered: export_dialog.open()
            }
            MenuSeparator {
            }
            MenuItem {
                text: "Settings"
                iconSource: "qrc:/images/images/preferences.png"
                onTriggered: {
                    settings_dialog.open()
                }
            }
        }
        Menu {
            title: "Automatic Playlists"

            PlaylistMenu {
                title: "Open"
                iconSource: "qrc:/images/images/open.png"
                playlistnames: auto_playlist_manager.autoPlaylistNames

                onSelected: {
                    playlist.addAutoPlaylist(listname)
                }
            }

            PlaylistMenu {
                title: "Edit"
                iconSource: "qrc:/images/images/edit.png"
                playlistnames: auto_playlist_manager.autoPlaylistNames

                onSelected: {
                    apd.clearAll()
                    var arr = auto_playlist_manager.getAutoPlaylist(listname)

                    apd.setApo1(arr[0].andor, arr[0].tag, arr[0].operator,
                                arr[0].value)

                    if (arr[1]) {
                        apd.setApo2(arr[1].andor, arr[1].tag, arr[1].operator,
                                    arr[1].value)
                    }

                    if (arr[2]) {
                        apd.setApo3(arr[2].andor, arr[2].tag, arr[2].operator,
                                    arr[2].value)
                    }

                    apd.apName = listname
                    apd.open()
                }
            }
            MenuItem {
                text: "Create"
                iconSource: "qrc:/images/images/new.png"
                onTriggered: {
                    apd.open()
                    apd.clearAll()
                }
            }
            PlaylistMenu {
                title: "Delete"
                iconSource: "qrc:/images/images/delete.png"
                playlistnames: auto_playlist_manager.autoPlaylistNames

                onSelected: auto_playlist_manager.deleteAutoPlaylist(listname)
            }
        }

        Menu {
            title: "Info"

            Menu {
                title: "Help"
                iconSource: "qrc:/images/images/help.png"
                MenuItem {
                    text: "Open the lalamachine wiki in your browser."
                    onTriggered: Qt.openUrlExternally(
                                     "https://bitbucket.org/r-mean/lalamachine/wiki/Home")
                }
            }

            Menu {
                title: "Report Bug"
                iconSource: "qrc:/images/images/bug.png"
                MenuItem {
                    text: "Create a new lalamachine issue in your browser."
                    onTriggered: Qt.openUrlExternally(
                                     "https://bitbucket.org/r-mean/lalamachine/issues/new")
                }
            }

            MenuItem {
                text: "About lalamachine"
                iconSource: "qrc:/images/images/info.png"
                onTriggered: infos.visible = true
            }
        }
    }

    AutoPlaylistManager {
        id: auto_playlist_manager
    }

    AutoPlaylistDialog {
        id: apd
        visible: false

        onAccepted: {
            if (!canQuit()) {
                return
            }

            var name = apd.apName
            var values = collectValues()
            auto_playlist_manager.saveAutoPlaylist(name, values)
            apd.close()
            playlist.addAutoPlaylist(name)
        }
    }

    InfoDialog {
        id: infos
        visible: false

        onAccepted: visible = false
    }

    SettingsDialog {
        id: settings_dialog
        visible: false
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

    Action {
        id: del_action
        shortcut: StandardKey.Delete
        tooltip: "Shortcut: " + shortcut
        onTriggered: playlist.deleteCurrentTrack()
    }

    Action {
        id: vol_up_action
        shortcut: "ctrl+up"
        tooltip: "Shortcut: " + shortcut
        onTriggered: volume_control.volumeUp()
    }

    Action {
        id: vol_down_action
        shortcut: "ctrl+down"
        tooltip: "Shortcut: " + shortcut
        onTriggered: volume_control.volumeDown()
    }

    Action {
        id: forward_action
        shortcut: StandardKey.Forward
        tooltip: "Shortcut: " + shortcut
        onTriggered: playlist.playNext()
    }

    Action {
        id: back_action
        shortcut: StandardKey.Back
        tooltip: "Shortcut: " + shortcut
        onTriggered: playlist.playPrevious()
    }

    Action {
        id: play_selected_action
        shortcut: "ctrl+return"
        tooltip: "Shortcut: " + shortcut
        onTriggered: playlist.playCurrentTrack()
    }

    Action {
        id: new_list_action
        text: "new playlist"
        shortcut: StandardKey.New
        tooltip: "Shortcut: " + shortcut
        onTriggered: playlist.clearList(true)
    }

    Action {
        id: play_pause_action
        shortcut: "ctrl+space"
        tooltip: "Shortcut: " + shortcut
        onTriggered: {
            if (playMusic.isPlaying) {
                playMusic.pause()
            } else {
                if (playMusic.hasAudio) {
                    playMusic.play()
                } else {
                    playlist.playNext()
                }
            }
        }
    }

    Action {
        id: open_action
        shortcut: StandardKey.Open
        tooltip: "Shortcut: " + shortcut
        onTriggered: fileDialog.visible = true
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
            if (Functions.millisToSec(position) > (Functions.millisToSec(
                                                       duration) - 1)) {
                playlist.playNext()
            }
        }

        loops: now_playing_container.repeatOne ? MediaPlayer.Infinite : 1

        function playTrack(path) {
            // Since the addition of the library it is necessary to
            // make sure an mrl is actually an mrl.
            playMusic.source = Functions.checkMrl(path)
            playMusic.play()
        }

        property bool isPlaying: playMusic.playbackState === MediaPlayer.PlayingState

        onError: {
            miss_dialog.text = errorString
            miss_dialog.text += "\nYou might want to delete that track"
            miss_dialog.text += "\nfrom the playlist and/or rescan you library."
            miss_dialog.open()
        }
    }

    TrackMissingDialog {
        id: miss_dialog
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

            Playlist {
                id: playlist
                isLibrary: false
                anchors.left: parent.left
                anchors.right: parent.right
                anchors.top: parent.top
                anchors.bottom: playlist_text.top

                repeatAll: now_playing_container.repeatAll
                random: now_playing_container.random

                nowPlayingSource: playMusic.source

                onStop: {
                    playMusic.stop()
                }

                onPlay: {
                    playMusic.playTrack(path)
                }

                function addAutoPlaylist(listname) {
                    playlist.createNewList()
                    playlist.addLib(auto_playlist_manager.getTracks(listname))
                }
            }

            Text {
                id: playlist_text
                anchors.left: parent.left
                anchors.leftMargin: 10
                anchors.right: parent.right
                anchors.bottom: parent.bottom
                height: 30
                color: "#ffffff"
                verticalAlignment: Text.AlignVCenter
                horizontalAlignment: Text.AlignLeft
                text: buildText()
                font.pointSize: 12
                font.family: "Helvetica"
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

    FileDialog {
        id: export_dialog
        visible: false
        title: "Please set target folder"
        selectMultiple: false
        selectExisting: false
        selectFolder: true

        onAccepted: {
            // We want to export the files into a folder 'playlist name'.
            var lalaname = currentPlaylist
            if (lalaname === miscPlaylistName) {
                lalaname = "new list"
            }
            playlist.exportPlaylist(export_dialog.folder + "/" + lalaname)
            close()
        }
        onRejected: {
            close()
        }
    }
}
