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
import QtQuick.Dialogs 1.2
import QtQuick.Window 2.2

import Lala 1.0

import "./functions.js" as Functions

Rectangle {
    id: playlist_container
    color: "darkblue"

    property bool isLibrary: false

    property string currentName
    property var playlistColumns: config.playlistColumns

    property int count: playlist_model.count
    property int rowPlaying: -1
    property string currentTitle: ""
    property url nowPlayingSource
    property string nowPlayingTitle: ""
    property bool repeatAll: false
    property alias totalPlaytimeString: tc.timestring

    property bool sortAsc: true

    signal play(string path, string title, string artist)
    signal stop
    signal showRightClickMenu

    // This setter is necessary to make sure that latest changes to the list are
    // remembered when a new playlist is loaded.
    function setCurrentPlaylist(name) {
        writeCurrentListIfNamed()
        currentName = name
    }

    function writeCurrentListIfNamed() {
        if (playlistIsNamed()) {
            writePlaylist(currentPlaylist)
        }
    }

    onCurrentNameChanged: {
        if (currentName != "") {
            readPlaylist(currentName)
        }
    }

    onRowPlayingChanged: {
        playlist_view.selection.clear()
        playlist_view.selection.select(rowPlaying)
        nowPlayingTitle = playlist_model.get(rowPlaying).title
    }

    MetadataProvider {
        id: meta
        function metaDataAsJson(path) {
            var cleanPath = path.replace("file:///", "");
            return JSON.parse(metaDataAsJsonString(cleanPath));
        }
    }

    Item {
        id: tc
        property int seconds: playlist_model.totalPlaytime
        readonly property string timestring: {
            Functions.secondsToDisplayString(tc.seconds);
        }
    }

    Exporter {
        id: file_ex

        onExportFinished: progress_window.close();

        function exportPlaylist(destination, files) {
            progress_window.show();
            exportFilesAsync(destination, JSON.stringify(files));
        }
    }

    Window {
        id: progress_window
        width: 300
        height: 100
        title: "Exporting..."
        visible: false

        ProgressBar {
            id: progress_bar
            width: 250
            height: 50
            anchors.centerIn: parent
            indeterminate: true

            Text {
                color: "white"
                anchors.centerIn: parent
                text: "Exporting..."
                font.pointSize: 20
                horizontalAlignment: Text.AlignHCenter
                verticalAlignment: Text.AlignVCenter
                style: Text.Outline
            }
        }
    }

    function playlistIsNamed() {
        return !(currentName === miscPlaylistName || currentName === "")
    }

    function showSaveDialog() {
        save_playlist_dialog.visible = true
    }

    function deletePlaylist(listname) {
        m3u.deletePlaylist(listname)
        if (listname === currentName) {
            createNewList()
        }
    }

    SavePlaylistDialog {
        id: save_playlist_dialog
        onAccepted: {
            writePlaylist(save_playlist_dialog.playlistName)
            save_playlist_dialog.visible = false
            setCurrentPlaylist(save_playlist_dialog.playlistName)
        }
    }

    function getPlaylistPath(name) {
        return m3u.playlistPath(name)
    }

    function playCurrentTrack() {
        playRow(playlist_view.currentRow)
    }

    // We can't bind the property to this function because the property will
    // also be set in function playRow(row)
    function updateNowPlayingRow() {
        for (var i = 0; i < playlist_model.count; ++i) {
            if (currentTitle == playlist_model.get(i)["title"]) {
                rowPlaying = i
                return
            }
        }
    }

    function addToPlaylist(listname) {
        addSelectionToPlaylist(listname)
        playlist_view.selection.clear()
    }

    // This function is adding the selection to the playlist.
    // This works from the playlist itself and from the library.
    // The library has options to add to a new playlist or the current one.
    // The main playlist can add stuff to a named playlist, even if to itself
    // if it is named.
    function addSelectionToPlaylist(listname) {
        // The main playlist (!isLibrary) can only add stuff to a named list.
        // This is by design, so we return here if it is an unnamed list
        // (miscPlaylistName).
        if (!isLibrary && listname === miscPlaylistName) {
            return
        }
        // The main playlist has no option to add stuff to a new list ("").
        // That is what the clear list button is for. It is debatable if that
        // could be considered a bug / missing functionality, but I am happy
        // with that behaviour.
        if (!isLibrary && listname === "") {
            return
        }
        // This is relevant if this is the library view. The library can create a new list/add to the misc playlist.
        if (listname === "") {
            mainPlaylist.createNewList()
            listname = miscPlaylistName
        }

        playlist_view.selection.forEach(function (rowIndex) {
            mainPlaylist.add(playlist_model.get(rowIndex).path)
        })

        updateAndSave()
    }

    function getSelectedRowIndices() {
        var indices = []

        playlist_view.selection.forEach(function (rowIndex) {
            indices.push(rowIndex)
        })

        return indices
    }

    function getSelectedTracks() {
        var indices = getSelectedRowIndices()
        var retval = []

        for (var i = 0; i < indices.length; ++i) {
            var row = indices[i]
            retval.push(playlist_model.get(row))
        }

        return retval
    }

    function clearSelection() {
        playlist_view.selection.clear()
    }

    function removeRows(rows) {
        for (var i = rows.length - 1; i >= 0; --i) {
            playlist_model.remove(rows[i])
        }

        playlist_model.updateTotalLength()
    }

    function deleteSelection() {
        var indices = getSelectedRowIndices()
        clearSelection()

        removeRows(indices)

        if (playlistIsNamed()) {
            writePlaylist(currentName)
        }
    }

    function exportPlaylist(path) {
        file_ex.exportPlaylist(path, getPathList())
    }

    function getPathList() {
        var list = []

        for (var i = 0; i < playlist_model.count; ++i) {
            list[i] = playlist_model.get(i)["path"]
        }

        return list
    }

    function writePlaylist(name) {
        m3u.writePlaylist(name, JSON.stringify(getPathList()))
    }

    function readPlaylist(name) {
        // We use this function here because we want to keep the currentName.
        emptyCurrentList()
        let files = JSON.parse(m3u.readPlaylist(name));

        for (var file of files) {
            add(file)
        }
    }

    function createNewList() {
        clearList(true)
    }
    function emptyCurrentList() {
        clearList(false)
    }
    function clearList(newlist) {
        if (newlist) {
            setCurrentPlaylist("")
        }
        playlist_model.clear()
        updateNowPlayingRow()
    }

    function addList(list) {
        for (var i = 0; i < list.length; ++i) {
            add(list[i].toString())
        }
    }

    function addJsonList(json, clear) {
        if (!json) {
            return
        }

        playlist_model.fromJson(json, clear)
        if (isLibrary) {
            playlist_model.defaultSort()
        }
        updateNowPlayingRow()
    }

    function add(path) {
        if (typeof (path) === "undefined" || path === "") {
            return
        }

        // First check the dbase here.
        var tmp = JSON.parse(musicLib.getMetadataForMrl(path))
        if (!tmp || !tmp.title) {
            // Only get the metadata from taglib if its not in the dbase
            // because it takes about forever.
            tmp = setId(meta.metaDataAsJson(path))
            console.log("INFO: Had to get metadate from taglib for", path)
        }

        playlist_model.appendTags(tmp)
        playlist_model.updateTotalLength()

    }

    function updateAndSave() {
        updateNowPlayingRow()

        if (playlistIsNamed()) {
            writePlaylist(currentName)
        }
    }

    // returns the provided JSON with an added id field to help find the correct
    // entry in the list later, after sorting and adding more stuff.
    // This is necessary because the mrl can't be the unique id, since a track
    // can be added multiple times.
    function setId(json) {
        json['id'] = playlist_model.count
        return json
    }

    function playRow(row) {
        rowPlaying = row
        currentTitle = playlist_model.get(row)["title"]
        play(playlist_model.get(row)["path"], currentTitle, playlist_model.get(row)["artist"])
        playlist_view.currentRow = rowPlaying
    }

    function hasNext() {
        return rowPlaying < playlist_model.count - 1
    }

    function getRandomNext() {
        var rand = rowPlaying
        while (rand === rowPlaying) {
            rand = Functions.randomInt(0, playlist_model.count - 1)
        }
        return rand
    }

    function playNext(random) {
        // Random has to be checked first because hasNext catches almost all.
        if (random) {
            console.log("playNext random")
            playRow(getRandomNext())
        } else if (hasNext()) {
            console.log("playNext has next")
            playRow(rowPlaying + 1)
        } else if (repeatAll) {
            console.log("playNext repeat all")
            playRow(0)
        } else {
            console.log("playlist finished")
        }
    }

    function hasPrevious() {
        return rowPlaying > 0
    }

    function playPrevious() {
        if (hasPrevious()) {
            playRow(rowPlaying - 1)
        }
    }

    function modelToArray(model) {
        var retval = []
        for (var i = 0; i < model.count; ++i) {
            retval[i] = model.get(i)
        }

        return retval
    }

    PlaylistModel {
        id: playlist_model
    }

    PlaylistTableView {
        id: playlist_view
        model: playlist_model
        anchors.fill: parent
    }
}
