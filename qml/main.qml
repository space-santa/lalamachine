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
import QtQuick.Dialogs 1.2
import QtQuick.Window 2.2
import QtQuick.Controls.Styles 1.4
import QtMultimedia 5.8

import Lala 1.0

import "./functions.js" as Functions

ApplicationWindow {
    id: master
    visible: true

    width: Screen.width / 2
    height: Screen.height / 2
    title: {
        var title = ""

        if (lalaplayer.currentTitle != "") {
            title += lalaplayer.currentTitle
            title += " | "
        }

        title += "lalamachine"

        return title
    }

    minimumHeight: 500
    minimumWidth: 500

    property alias musicLib: libview.musicLib
    property string miscPlaylistName: "cs1m090"
    property string currentPlaylist: playlist.currentName
    property LalaPlaylist mainPlaylist: playlist
    property PlaylistProvider m3u: m3u
    property Configuration config: config

    // lalaplayer is globally accessible to change volume, etc
    property MediaPlayer lalaplayer: playMusic

    property bool playingStatus: getPlayingStatus()

    function getPlayingStatus() {
        if (playMusic.isPlaying) {
            return true
        } else {
            return false
        }
    }

	onXChanged: {
		if (settings.isActive) {
			settings.x = master.x
		}
	}
	onYChanged: {
		if (settings.isActive) {
			settings.y = master.y
		}
	}
	onHeightChanged: {
		if (settings.isActive) {
			settings.height = master.height
		}
	}
	onWidthChanged: {
		if (settings.isActive) {
			settings.width = master.width
		}
	}

    Component.onCompleted: {
        playlist.setCurrentPlaylist(config.lastPlaylist)

		if (settings.x > 0) {
			master.x = settings.x
		}
		if (settings.y > 0) {
			master.y = settings.y
		}
		if (settings.height > 0) {
			master.height = settings.height
		}
		if (settings.width > 0) {
			master.width = settings.width
		}
		settings.isActive = true

		console.log(settings.height, settings.width, settings.writable)

        if (settings.showPlaylist) {
            show_list_action.trigger()
        } else {
            show_musiclib_action.trigger()
        }
    }

    Settings {
        id: settings
        property bool showPlaylist: show_list.checked
    }

    onClosing: {
        playlist.writePlaylist(miscPlaylistName)

        // If the current list has a name we want to remember latest changes.
        playlist.writeCurrentListIfNamed()

        // I am deliberatly not checking for a named playlist here.
        // If the currentPlaylist is not empty but the miscPlaylistName it still
        // as expected because then the lastPlaylist is set to the
        // miscPlaylistName in the else branch.
        // FIXME: I don't understand the previous comment. That means this is
        // probably not a good way to do things because if I don't understand
        // it I can't maintain it. => TODO: REFACTOR
        if (currentPlaylist === "") {
            config.lastPlaylist = miscPlaylistName
        } else {
            config.lastPlaylist = currentPlaylist
        }
    }

    Action {
        id: quit_action
        shortcut: "ctrl+q"
        tooltip: "Shortcut: " + shortcut
        onTriggered: master.close()
    }

    menuBar: MenuBar {
        id: menu_bar
        __contentItem.height: 25
        __contentItem.transform: Scale {
            yScale: 1
        }
        MusicLibMenu {
            onShowSetLibraryDialog: lib_dialog.visible = true
            onRescanLibrary: libview.rescan()
        }
        MainPlaylistMenu {
            saveAction: save_current_list_action
            onOpenPlaylist: playlist.setCurrentPlaylist(listname)
            onSaveCurrentPlaylist: playlist.writeCurrentListIfNamed()
            onShowSavePlaylistDialog: playlist.showSaveDialog()
            onDeletePlaylist: playlist.deletePlaylist(listname)
            onOpenExportDialog: export_dialog.open()
            onOpenSettingsDialog: settings_dialog.open()
        }
        InfoMenu {
            onShowInfoDialog: infos.visible = true
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

    PlaylistProvider {
        id: m3u
    }

    Action {
        id: save_current_list_action
        shortcut: StandardKey.save
        tooltip: "Shortcut: " + shortcut
        onTriggered: playlist.writeCurrentListIfNamed()
        enabled: playlist.playlistIsNamed()
        iconSource: "../images/save.png"
    }

    Action {
        id: del_action
        shortcut: StandardKey.Delete
        tooltip: "Shortcut: " + shortcut
        onTriggered: playlist.deleteSelection()
    }

    Action {
        id: vol_up_action
        shortcut: "ctrl+up"
        tooltip: "Shortcut: " + shortcut
        onTriggered: player_controls.volumeUp()
    }

    Action {
        id: vol_down_action
        shortcut: "ctrl+down"
        tooltip: "Shortcut: " + shortcut
        onTriggered: player_controls.volumeDown()
    }

    Action {
        id: forward_action
        shortcut: StandardKey.Forward
        tooltip: "Shortcut: " + shortcut
        onTriggered: playMusic.playNextTrack()
    }

    Action {
        id: back_action
        shortcut: StandardKey.Back
        tooltip: "Shortcut: " + shortcut
        onTriggered: playMusic.playPreviousTrack()
    }

    Action {
        id: play_selected_action
        shortcut: "ctrl+return"
        tooltip: "Shortcut: " + shortcut
        onTriggered: playMusic.playCurrentTrack()
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

    Configuration {
        id: config
    }

    MediaPlayer {
        id: playMusic

        property string currentTitle
        property string currentArtist
		property bool isPlaying: playMusic.playbackState == MediaPlayer.PlayingState
		loops: player_controls.repeatOne ? MediaPlayer.Infinite : 1
		audioRole: MediaPlayer.MusicRole
		property bool currentTrackIsAtEnd: duration - position < 100

        onStopped: {
			if (currentTrackIsAtEnd) {
				playNextTrack()
			}
        }

        function playNextTrack() {
            if (left_shelve.z > right_shelve.z) {
                left_shelve.playNext(player_controls.random)
            } else {
                right_shelve.playNext(player_controls.random)
            }
        }

        function playPreviousTrack() {
            if (left_shelve.z > right_shelve.z) {
                left_shelve.playPrevious()
            } else {
                right_shelve.playPrevious()
            }
        }

        function playCurrentTrack() {
            if (left_shelve.z > right_shelve.z) {
                left_shelve.playCurrent()
            } else {
                right_shelve.playCurrent()
            }
        }

        function playTrack(path, title, artist) {
			var pathWithoutBackSlash = path.replace(/\\/g, "/");
			playMusic.source = pathWithoutBackSlash
            playMusic.play()
            currentTitle = title
            currentArtist = artist
        }

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

    ExclusiveGroup {
        id: tab_group

        Action {
            id: show_list_action
            checkable: true
            shortcut: "ctrl+h"
            text: "Playlist"
            tooltip: "Shortcut: " + shortcut
            onTriggered: {
                left_shelve.z = 3
                right_shelve.z = 1
            }
        }

        Action {
            id: show_musiclib_action
            checkable: true
            shortcut: "ctrl+l"
            tooltip: "Shortcut: " + shortcut
            text: "Library"
            onTriggered: {
                left_shelve.z = 1
                right_shelve.z = 3
            }
        }
    }

    Item {
        id: tab_container
        anchors.top: parent.top
        anchors.bottom: player_controls.top
        anchors.right: parent.right
        anchors.left: parent.left

        Item {
            id: tab_bar
            z: 5
            anchors.top: parent.top
            anchors.left: parent.left
            anchors.right: parent.right
            height: 45

            Button {
                id: show_list
                anchors.top: parent.top
                anchors.left: parent.left
                anchors.bottom: parent.bottom
                width: parent.width / 2
                action: show_list_action

                style: ButtonStyle {
                    label: TextBase {
                        text: control.text
                    }
                }
            }

            Button {
                id: show_musiclib
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                anchors.right: parent.right
                anchors.left: show_list.right
                action: show_musiclib_action

                style: ButtonStyle {
                    label: TextBase {
                        text: control.text
                    }
                }
            }
        }

        Rectangle {
            id: shield
            color: "darkblue"

            anchors.fill: parent
            z: 2

            MouseArea {
                id: catchall
                anchors.fill: parent
            }
        }

        Rectangle {
            id: left_shelve
            z: 3
            anchors.left: parent.left
            anchors.top: tab_bar.bottom
            anchors.right: parent.right
            anchors.bottom: parent.bottom
            color: "transparent"

            function playNext(random) {
                playlist.playNext(random)
            }

            function playPrevious() {
                playlist.playPrevious()
            }

            function playCurrent() {
                playlist.playCurrentTrack()
            }

            Rectangle {
                id: playlist_container
                anchors.fill: parent
                color: "transparent"

                MainPlaylist {
                    id: playlist
                    isLibrary: false
                    anchors.left: parent.left
                    anchors.right: parent.right
                    anchors.top: parent.top
                    anchors.bottom: playlist_text.top
                    repeatAll: player_controls.repeatAll
                    nowPlayingSource: playMusic.source

                    onAddTracksToNamedPlaylist: m3u.addTracksToNamedPlaylist(
                                                    listname, tracks)

                    onStop: {
                        playMusic.stop()
                    }

                    onPlay: {
                        playMusic.playTrack(path, title, artist)
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
        }

        Rectangle {
            id: right_shelve
            z: 1
            anchors.left: parent.left
            anchors.top: tab_bar.bottom
            anchors.right: parent.right
            anchors.bottom: parent.bottom
            color: "transparent"

            function playNext(random) {
                libview.playNext(random)
            }

            function playPrevious() {
                libview.playPrevious()
            }

            function playCurrentTrack() {
                libview.playCurrentTrack()
            }

            LibraryView {
                id: libview
                anchors.fill: parent
                library: config.libPath

                onAddTrack: {
                    playlist.add(path)
                    playlist.updateAndSave()
                }
            }
        }
    }

    PlayerControls {
        id: player_controls
        anchors.bottom: parent.bottom
        anchors.right: parent.right
        anchors.left: parent.left

        rawTitle: lalaplayer.currentTitle
        rawAlbumArtist: lalaplayer.currentArtist
        duration: lalaplayer.duration
        position: lalaplayer.position
        hasAudio: lalaplayer.hasAudio

        gradient: Gradient {
            GradientStop {
                position: 0
                color: "darkblue"
            }
            GradientStop {
                position: 1.00
                color: "lightblue"
            }
        }

        onSeek: {
            lalaplayer.seek(pos)
        }
    }

    FileDialog {
        id: fileDialog
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
