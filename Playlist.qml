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

import Lala 1.0

import "qrc:/functions.js" as Functions

Rectangle {
    id: playlist_container
    color: "transparent"

    property bool isLibrary: false

    property int count: playlist_model.count
    property int rowPlaying: -1
    property int currentId: -1
    property url nowPlayingSource
    property string nowPlayingTitle: ""
    property bool repeatAll: false
    property alias totalPlaytimeString: tc.timestring

    property alias provideTotalTime: tc.enabled

    signal play(string path)
    signal stop

    onRowPlayingChanged: {
        playlist_view.selection.clear()
        playlist_view.selection.select(rowPlaying)
        nowPlayingTitle = playlist_model.get(rowPlaying).title
    }

    Metadata {
        id: meta
    }

    M3uInOut {
        id: m3u_inout
    }

    TimeConverter {
        id: tc
        seconds: totalPlaytime()
    }

    function showSaveDialog() {
        save_playlist_dialog.visible = true
    }

    function deletePlaylist(listname) {
        m3u.deletePlaylist(listname)
    }

    SavePlaylistDialog {
        id: save_playlist_dialog
        onAccepted: {
            writePlaylist(save_playlist_dialog.playlistName)
            save_playlist_dialog.visible = false
        }
    }

    PlaylistSorter {
        id: listsorter
    }

    function getPlaylistPath(name) {
        return m3u_inout.m3uPath(name)
    }

    function totalPlaytime() {
        if (tc.enabled) {
            var lenght = 0
            for (var i = 0; i < playlist_model.count; ++i) {
                lenght += playlist_model.get(i).length
            }
            return lenght
        } else {
            return 0
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

        for (var i = indices.length - 1; i >= 0; --i) {
            playlist_model.remove(indices[i])
        }
    }

    function playCurrentTrack() {
        playRow(playlist_view.currentRow)
    }

    // FIXME: There should be a way to bind the property to this function.
    function updateNowPlayingRow() {
        for (var i = 0; i < playlist_model.count; ++i) {
            if (currentId == playlist_model.get(i).id) {
                console.log("true")
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

        playlist_view.selection.forEach(function (rowIndex) {
            if (listname === miscPlaylistName) {
                play(playlist_model.get(rowIndex).path)
            } else {
                m3u_inout.addToPlaylist(playlist_model[rowIndex].path, listname)
            }
            console.log(rowIndex)
        })
    }

    function writePlaylist(name) {
        var list = []

        for (var i = 0; i < playlist_model.count; ++i) {
            list[i] = playlist_model.get(i)["path"]
        }

        m3u_inout.writePlaylist(name, list)
    }

    function readPlaylist(name) {
        clearList()
        addList(m3u_inout.readPlaylist(name))
    }

    function clearList() {
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

        playlist_model.append(setId(meta.metaData(path)))

        updateNowPlayingRow()
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

    function playNext() {
        if (hasNext()) {
            console.log("playNext has next")
            playRow(rowPlaying + 1)
        } else if (repeatAll) {
            console.log("playNext repeat")
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

        var sortwhat
        var sorthow

        if (what === 0) {
            sortwhat = PlaylistSorter.TRACK
        }
        if (what === 1) {
            sortwhat = PlaylistSorter.TITLE
        }
        if (what === 2) {
            sortwhat = PlaylistSorter.COMMENT
        }
        if (what === 3) {
            sortwhat = PlaylistSorter.LENGTH
        }
        if (what === 4) {
            sortwhat = PlaylistSorter.GENRE
        }
        if (what === 5) {
            sortwhat = PlaylistSorter.ARTIST
        }

        if (how === 0) {
            sorthow = PlaylistSorter.ASCENDING
        } else {
            sorthow = PlaylistSorter.DESCENDING
        }

        replaceJson(listsorter.sort(modelToArray(playlist_model),
                                    sortwhat, sorthow))

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
        playlistnames: m3u_inout.playlistNames

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

        rowDelegate: Item {
            Rectangle {
                anchors {
                    left: parent.left
                    right: parent.right
                    verticalCenter: parent.verticalCenter
                }
                height: parent.height
                color: styleData.selected ? 'darkslategrey' : "transparent"
                MouseArea {
                    anchors.fill: parent
                    acceptedButtons: Qt.RightButton
                    onClicked: {
                        if (mouse.button == Qt.RightButton) {
                            console.log("Right-Click", styleData.row)
                            if (playlist_view.selection.count < 2) {
                                playlist_view.selection.clear()
                                playlist_view.selection.select(styleData.row)
                            }
                        }
                        rcm.popup()
                    }
                }
            }
        }
    }
}
