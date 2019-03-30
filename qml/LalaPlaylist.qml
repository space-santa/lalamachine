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

    property int count: playlist_model.count
    property int rowPlaying: -1
    property int currentId: -1
    property url nowPlayingSource
    property string nowPlayingTitle: ""
    property bool repeatAll: false
    property alias totalPlaytimeString: tc.timestring

    property bool sortAsc: true

    signal play(string path, string title, string artist)
    signal stop
    signal showRightClickMenu

    // This setter is necessary to make sure that latest changes to the list are
    // remebered when a new playlist is loaded.
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

    Item {
        id: meta
        function metaDataAsJson(path) {
            var res = path.replace("file:///", "");
            res = res.trim();
            res = res.replace(/\s/g, "%20");
            res = res.replace(/\//g, "%5C");
            var theUrl = "http://localhost:5003/api/tag/" + res;
            console.log(theUrl);
            var xmlHttp = new XMLHttpRequest();
            xmlHttp.open("GET", theUrl, false ); // false for synchronous request
            xmlHttp.send();
            var rawResponse = xmlHttp.response;
            console.log(rawResponse);
            var response = JSON.parse(rawResponse);
            response["mrl"] = path.replace("file:///", "");
            response["path"] = path.replace("file:///", "");
            response["artist"] = response["artist"].join(", ");
            response["genre"] = response["genre"].join(", ");
            return response;
        }
    }

    Item {
        id: tc
        property int seconds: playlist_model.totalPlaytime
        readonly property string timestring: {
            Functions.secondsToDisplayString(tc.seconds);
        }
    }

    Item {
        id: file_ex

        function exportPlaylist(destination, files) {
            destination = destination.replace("file:///", "")
            var xhttp = new XMLHttpRequest();
            xhttp.open("POST", "http://localhost:5003/api/fileexport/", true);
            xhttp.setRequestHeader("Content-type", "application/json");
            xhttp.onreadystatechange = function() {
                console.log("readyState", this.readyState);
                console.log("status", this.status);
                progress_window.close();
            };
            var data = {};
            data["Destination"] = destination;
            data["Files"] = files;
            console.log(JSON.stringify(data))
            xhttp.send(JSON.stringify(data));
            onStarted: progress_window.show()
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
            if (currentId == playlist_model.get(i).id) {
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

    function getPathList() {
        var list = []

        for (var i = 0; i < playlist_model.count; ++i) {
            list[i] = playlist_model.get(i)["path"]
        }

        return list
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

    function writePlaylist(name) {
        m3u.writePlaylist(name, playlist_model.toJson())
    }

    function readPlaylist(name) {
        // We use this function here because we want to keep the currentName.
        emptyCurrentList()
        addJsonList(m3u.readPlaylist(name))
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

    function addJsonList(json) {
        var d = Date.now()
        if (json[0].toString() == "OLDFORMAT") {
            json.shift()
            addList(json)
        } else {
            playlist_model.fromJson(json)
        }
        updateNowPlayingRow()
        console.log("Adding the list took", Date.now() - d, "ms.")
    }

    function add(path) {
        if (typeof (path) === "undefined" || path === "") {
            return
        }

        // First check the dbase here.
        var tmp = musicLib.getMetadataForMrl(path)
        if (!tmp || !tmp.title) {
            // Only get the metadata from taglib if its not in the dbase
            // because it takes about forever.
            tmp = setId(meta.metaDataAsJson(path))
            console.log("INFO: Had to get metadate from taglib for", path)
        }

        playlist_model.append(tmp)
    }

    function updateAndSave() {
        updateNowPlayingRow()

        if (playlistIsNamed()) {
            writePlaylist(currentName)
        }
    }

    function addLib(json) {
        playlist_model.setLibrary(json)
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
        play(playlist_model.get(row)["mrl"], playlist_model.get(row)["title"],
             playlist_model.get(row)["artist"])
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

    function pathList() {
        return playlist_model.pathList()
    }

    ListModel {
        id: playlist_model
        property int totalPlaytime: 0

        function setLibrary(json){
            for (var i = 0; i < json.length; ++i) {
                append(json[i]);
            }
        }
        function sortRole(role, order) {}
        function toJson() {}
        function fromJson(json) {
            clear();
            setLibrary(json);
        }
        function pathList() {}
    }

    TableView {
        id: playlist_view
        anchors.fill: parent
        model: playlist_model
        selectionMode: SelectionMode.ExtendedSelection
        backgroundVisible: true

        style: TableStyle {
        }

        property var playlistColumns: config.playlistColumns

        // These values are required for drag and move. For a explanation,
        // see TableViewDelegate.qml.
        property int mouseY
        property int containerHeight
        property int initialRow

        Component.onCompleted: setColumns()
        //onPlaylistColumnsChanged: setColumns()

        // Since we are handling left clicks in the delegate we are effectively
        // disabling ctrl-click/shift-click here.
        // Hence a naive attempt to implement that functionality manually.
        // When clicked into the table, we force active focus to get key events.
        // Hitting ctrl/shift with focus sets the appropriat flag to true.
        // Releasing the key sets its flag to false.
        // Hitting e.g. shift while still holding ctrl will set ctrl to false.
        // That is the highlander principle. There can only be one.
        property bool ctrlPressed: false
        property bool shiftPressed: false

        // The lastSelection property is required for shift-click action.
        // We need to now the range to select.
        property int lastSelection: -1

        // We need a flag to determine if the mouse button is currently pressed.
        // If it is not pressed, we can't have a drag action.
        property bool leftPressed: false

        onActiveFocusChanged: {
            if (!focus) {
                ctrlPressed = false
                shiftPressed = false
            }
        }

        Keys.onPressed: {
            if (event.key === Qt.Key_Control) {
                shiftPressed = false
                ctrlPressed = true
            }
            if (event.key === Qt.Key_Shift) {
                ctrlPressed = false
                shiftPressed = true
            }
        }

        Keys.onReleased: {
            if (event.key === Qt.Key_Control) {
                ctrlPressed = false
            }
            if (event.key === Qt.Key_Shift) {
                shiftPressed = false
            }
        }

        onMouseYChanged: {
            // Since the library can't play tracks in order,
            // moving them around doesn't make sense.
            if (isLibrary) {
                return
            }
            // If ctrl or shift is pressed it's not dragging, it's selecting.
            if (ctrlPressed || shiftPressed) {
                return
            }

            // If the mouse button is not pressed, no drag.
            if (!leftPressed) {
                return
            }

            var newRow = initialRow + mouseY / containerHeight
            console.log("Current Row", currentRow, "New Row", newRow)

            if (newRow < 0) {
                return
            }

            playlist_model.move(currentRow, newRow, 1)
            // Update the currentRow so the next drag n move works.
            currentRow = newRow
            // Update the selected row to follow the drag.
            selection.clear()
            selection.select(currentRow)
        }

        onClicked: {
            forceActiveFocus()
            currentRow = row
            if (!ctrlPressed) {
                selection.clear()
            }

            if (shiftPressed) {
                selection.select(lastSelection, row)
            } else {
                selection.select(row)
                lastSelection = row
            }
        }

        function setColumns() {
            for (var i = columnCount - 1; i >= 0; --i) {
                removeColumn(i)
            }

            let columnsJson = JSON.parse(playlistColumns);

            for (let i = 0; i < columnsJson.length; ++i) {
                if (columnsJson[i].value) {
                    let columnString = buildColumnString(columnsJson[i].key);
                    var o = Qt.createQmlObject(columnString, playlist_view, "DynO");
                    playlist_view.addColumn(o)
                }
            }
        }

        function buildColumnString(tag) {
            var width = 150
            var title = tag

            if (tag === "track") {
                width = 70
            }
            if (tag === "title") {
                width = 250
            }
            if (tag === "comment") {
                width = 100
            }
            if (tag === "length") {
                width = 80
                tag = "lengthString"
            }
            if (tag === "genre") {
                width = 150
            }
            if (tag === "artist") {
                width = 150
            }
            if (tag === "album") {
                width = 150
            }
            if (tag === "year") {
                width = 70
            }
            if (tag === "discNumber") {
                width = 50
                title = "Disc"
            }

            return columnString(tag, title, width)
        }

        function columnString(role, title, width) {
            var retval = "import QtQuick 2.0;\n"
            retval += "import QtQuick.Controls 1.2;\n"
            retval += "TableViewColumn {\n"
            retval += "role: \"" + role + "\";\n"
            retval += "title: \"" + title + "\";\n"
            retval += "width: " + width + "\n"
            retval += "}"

            return retval
        }

        onModelChanged: {
            if (playlist_model.rowCount() > 0) {
                playlist_view.resizeColumnsToContents()
                updateNowPlayingRow()
            }
        }

        function getColumnRole(col) {
            return getColumn(col).role
        }

        sortIndicatorVisible: true
        onSortIndicatorColumnChanged: {
            console.log(getColumnRole(sortIndicatorColumn))
            playlist_model.sortRole(getColumnRole(sortIndicatorColumn),
                                    sortIndicatorOrder)
        }
        onSortIndicatorOrderChanged: {
            console.log(getColumnRole(sortIndicatorColumn))
            playlist_model.sortRole(getColumnRole(sortIndicatorColumn),
                                    sortIndicatorOrder)
        }

        rowDelegate: TableViewDelegate {
            target: playlist_view
            onRightClick: showRightClickMenu()

            onDoubleClicked: {
                stop()
                playRow(row)
            }

            onPressed: playlist_view.leftPressed = true
            onReleased: playlist_view.leftPressed = false

            onMouseYChanged: {
                //console.log("MOUSE Y CHANGED", y, height, row)
                playlist_view.mouseY = y
                playlist_view.containerHeight = height
                playlist_view.initialRow = row
            }
        }

        itemDelegate: ItemDelegate {
        }

        headerDelegate: TableHeaderDelegate {
        }
    }
}
