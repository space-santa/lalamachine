import QtQuick 2.0
import QtQuick.Controls 1.2

import Lala 1.0

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

    Metadata {
        id: meta
    }

    function clearList() {
        playlist_model.clear()
    }

    function addList(list) {
        for (var i = 0; i < list.length; ++i) {
            add(list[i].toString())
        }
    }

    function add(path) {
        playlist_model.append(meta.metaData(path))
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
        TableViewColumn{ role: "title"  ; title: "title"}
        TableViewColumn{ role: "comment"  ; title: "comment"}
        TableViewColumn{ role: "genre"  ; title: "genre"}
        TableViewColumn{ role: "artist"  ; title: "artist"}

        onModelChanged: playlist_view.resizeColumnsToContents()

        onDoubleClicked: playRow(row)
    }
}
