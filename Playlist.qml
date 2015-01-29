import QtQuick 2.0
import QtQuick.Controls 1.2

Rectangle {
    property int rowPlaying: -1
    onRowPlayingChanged: {
        playlist_view.selection.clear()
        playlist_view.selection.select(rowPlaying)
    }

    signal play(string path)

    Keys.onDeletePressed: {
        playlist_model.remove(playlist_view.currentRow)
    }

    function clearList() {
        playlist_model.clear()
    }

    function add(path) {
        playlist_model.append({mrl: path})
    }

    function playRow(row) {
        rowPlaying = row
        play(playlist_model.get(row)["mrl"])
    }

    function hasNext() {
        return rowPlaying < playlist_model.count - 1
    }

    function playNext() {
        if (hasNext()) {
            playRow(rowPlaying + 1)
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

    ListModel {
        id: playlist_model
    }

    TableView {
        id: playlist_view
        anchors.fill: parent
        model: playlist_model
        TableViewColumn{ role: "mrl"  ; title: "path" ; width: parent.width}

        onModelChanged: playlist_view.resizeColumnsToContents()

        onDoubleClicked: playRow(row)
    }
}
