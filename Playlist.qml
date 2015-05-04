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

import "qrc:/functions.js" as Functions

Rectangle {
    id: playlist_container
    color: "transparent"

    property bool isLibrary: false

    property string currentName

    property int count: playlist_model.count
    property int rowPlaying: -1
    property int currentId: -1
    property url nowPlayingSource
    property string nowPlayingTitle: ""
    property bool repeatAll: false
    property bool random: false
    property alias totalPlaytimeString: tc.timestring

    property int sortwhat: MusicLib.ARTIST
    property bool sortAsc: true

    signal play(string path)
    signal stop

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

    Metadata {
        id: meta
    }

    TimeConverter {
        id: tc
        seconds: totalPlaytime()
    }

    FileExporter {
        id: file_ex

        onStarted: progress_window.show()
        onFinished: progress_window.close()

        onUpdateProgress: {
            progress_bar.value = val
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
            minimumValue: 0
            maximumValue: 100

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
            clearList(true)
        }
    }

    SavePlaylistDialog {
        id: save_playlist_dialog
        onAccepted: {
            writePlaylist(save_playlist_dialog.playlistName)
            save_playlist_dialog.visible = false
            currentName = save_playlist_dialog.playlistName
        }
    }

    PlaylistSorter {
        id: listsorter
    }

    function getPlaylistPath(name) {
        return m3u.m3uPath(name)
    }

    function totalPlaytime() {
        if (isLibrary) {
            return lib.totalLength
        } else {
            var lenght = 0
            for (var i = 0; i < playlist_model.count; ++i) {
                lenght += playlist_model.get(i).length
            }
            return lenght
        }
    }

    function deleteCurrentTrack() {
        if (isLibrary) {
            return
        }

        var indices = []

        playlist_view.selection.forEach(function (rowIndex) {
            indices.push(rowIndex)
        })

        playlist_view.selection.clear()

        for (var i = indices.length - 1; i >= 0; --i) {
            playlist_model.remove(indices[i])
        }

        if (playlistIsNamed()) {
            writePlaylist(currentName)
        }
    }

    function playCurrentTrack() {
        playRow(playlist_view.currentRow)
    }

    // FIXME: There should be a way to bind the property to this function.
    function updateNowPlayingRow() {
        for (var i = 0; i < playlist_model.count; ++i) {
            if (currentId == playlist_model.get(i).id) {
                rowPlaying = i
                return
            }
        }
    }

    function addSelectionToPlaylist(listname) {
        // We are not going to add to ourselve.
        if (!isLibrary && listname === miscPlaylistName) {
            return
        }
        // FIXME: We are not going to delete ourselve.
        // The problem is that once the list is cleared, the logic below
        // doesn't make sense because we need the listmodel to get the
        // information. Should be fixed in the future, but right now that is
        // not important enough for me to put real effort in it.
        if (!isLibrary && listname === "") {
            return
        }

        if (listname === "") {
            mainPlaylist.clearList(true)
            listname = miscPlaylistName
        }

        playlist_view.selection.forEach(function (rowIndex) {
            if (listname === miscPlaylistName || listname === currentPlaylist) {
                mainPlaylist.add(playlist_model.get(rowIndex).path)
            } else {
                m3u.addToPlaylist(playlist_model.get(rowIndex).path, listname)
            }
        })
    }

    function getPathList() {
        var list = []

        for (var i = 0; i < playlist_model.count; ++i) {
            list[i] = playlist_model.get(i)["path"]
        }

        return list
    }

    function exportPlaylist(path) {
        file_ex.exportPlaylist(path, getPathList())
    }

    function writePlaylist(name) {
        m3u.writePlaylist(name, getPathList())
    }

    function readPlaylist(name) {
        clearList(false)
        addList(m3u.readPlaylist(name))
    }

    function clearList(newlist) {
        if (newlist) {
            currentName = ""
        }
        playlist_model.clear()
        updateNowPlayingRow()
    }

    function addList(list) {
        for (var i = 0; i < list.length; ++i) {
            add(list[i].toString())
        }
    }

    function add(path) {
        if (typeof (path) === "undefined" || path === "") {
            return
        }

        playlist_model.append(setId(meta.metaDataAsJson(path)))

        updateNowPlayingRow()

        if (playlistIsNamed()) {
            writePlaylist(currentName)
        }
    }

    function addLib(json) {
        for (var i in json) {
            playlist_model.append(setId(json[i]))
        }
    }

    function replaceJson(json) {
        playlist_model.clear()
        for (var i in json) {
            playlist_model.append(json[i])
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
        currentId = playlist_model.get(row).id
        play(playlist_model.get(row)["mrl"])
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

    function playNext() {
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

    // what can be one of the keys
    //   0 = track
    //   1 = title
    //   2 = comment
    //   3 = lengthString
    //   4 = genre
    //   5 = artist
    // how is ascending (=0) or descending (=1)
    // WARNING! DANGER! Just negating the ascending function doesn't work.
    // Then it would try to sort equal which leads to an infinite loop.
    // We would check that something is not smaller than the other and then
    // swap those values. This would always be true if we have the equal
    // in there.
    function sort(what, how) {
        var startdate = Date.now()

        var sorthow

        if (what === 0) {
            sortwhat = MusicLib.TRACK
        }
        if (what === 1) {
            sortwhat = MusicLib.TITLE
        }
        if (what === 2) {
            sortwhat = MusicLib.COMMENT
        }
        if (what === 3) {
            sortwhat = MusicLib.LENGTH
        }
        if (what === 4) {
            sortwhat = MusicLib.GENRE
        }
        if (what === 5) {
            sortwhat = MusicLib.ARTIST
        }

        if (how === 0) {
            sortAsc = true
            sorthow = PlaylistSorter.ASCENDING
        } else {
            sortAsc = false
            sorthow = PlaylistSorter.DESCENDING
        }

        if (!isLibrary) {
            replaceJson(listsorter.sort(modelToArray(playlist_model),
                                        sortwhat, sorthow))
        }

        updateNowPlayingRow()
        var enddate = Date.now()

        console.log("TOTAL TIME IN MS", enddate - startdate)
    }

    function moveTop() {
        playlist_model.move(playlist_view.currentRow, 0, 1)
    }
    function moveUp() {
        playlist_model.move(playlist_view.currentRow,
                            playlist_view.currentRow - 1, 1)
    }
    function moveDown() {
        playlist_model.move(playlist_view.currentRow,
                            playlist_view.currentRow + 1, 1)
    }
    function moveBottom() {
        playlist_model.move(playlist_view.currentRow,
                            playlist_model.count - 1, 1)
    }

    RightClickMenu {
        id: rcm

        isLibrary: playlist_container.isLibrary

        onAddToPlaylist: {
            addSelectionToPlaylist(listname)
            playlist_view.selection.clear()
        }

        onDeleteSelection: deleteCurrentTrack()
    }

    ListModel {
        id: playlist_model
        onRowsMoved: {
            updateNowPlayingRow()
        }
    }

    TableView {
        id: playlist_view
        anchors.fill: parent
        model: playlist_model
        selectionMode: SelectionMode.ContiguousSelection

        TableViewColumn {
            role: "track"
            title: "track"
            width: 50
        }
        TableViewColumn {
            role: "title"
            title: "title"
            width: 200
        }
        TableViewColumn {
            role: "comment"
            title: "comment"
            width: 100
        }
        TableViewColumn {
            role: "lengthString"
            title: "length"
            width: 100
        }
        TableViewColumn {
            role: "genre"
            title: "genre"
            width: 150
        }
        TableViewColumn {
            role: "artist"
            title: "artist"
            width: 150
        }

        onModelChanged: {
            playlist_view.resizeColumnsToContents()
            updateNowPlayingRow()
        }

        onDoubleClicked: {
            stop()
            playRow(row)
        }

        sortIndicatorVisible: true
        onSortIndicatorColumnChanged: {
            console.log(sortIndicatorColumn)
            sort(sortIndicatorColumn, sortIndicatorOrder)
        }
        onSortIndicatorOrderChanged: {
            console.log(sortIndicatorOrder)
            sort(sortIndicatorColumn, sortIndicatorOrder)
        }

        rowDelegate: TableViewDelegate {
            target: playlist_view
            onRightClick: rcm.popup()
        }
    }
}
