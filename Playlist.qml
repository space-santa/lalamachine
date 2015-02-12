import QtQuick 2.0
import QtQuick.Controls 1.2

import Lala 1.0

Rectangle {
    property int rowPlaying: -1
    property url nowPlayingSource
    property bool repeatAll: false
    property alias savePlaylistVisible: save_playlist_dialog.visible

    signal play(string path)
    signal stop

    onRowPlayingChanged: {
        playlist_view.selection.clear()
        playlist_view.selection.select(rowPlaying)
    }

    Metadata {
        id: meta
    }

    M3uInOut {
        id: m3u_inout
    }

    SavePlaylistDialog {
        id: save_playlist_dialog
        onAccepted: {
            writePlaylist(save_playlist_dialog.playlistName)
            save_playlist_dialog.visible = false
        }
    }

    function deleteCurrentTrack() {
        playlist_model.remove(playlist_view.currentRow)
    }

    function playCurrentTrack() {
        playRow(playlist_view.currentRow)
    }

    function updateNowPlayingRow() {
        var row = -1
        for (var i = 0; i < playlist_model.count; ++i) {
            if (nowPlayingSource == playlist_model.get(i)["mrl"]) {
                console.log("true")
                row = i
            }
        }

        rowPlaying = row
    }

    function writePlaylist(name) {
        var list = []

        for (var i = 0; i < playlist_model.count; ++i) {
            list[i] = playlist_model.get(i)["path"]
        }

        m3u_inout.writePlaylist(name, list)
    }

    function readPlaylist(name) {
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
        playlist_model.append(meta.metaData(path))

        updateNowPlayingRow()
    }

    function playRow(row) {
        rowPlaying = row
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
        var sorter
        if (what === 0) {
            if (how === 0) {
                sorter = trackAsc
            } else {
                sorter = trackDes
            }
        } else if (what === 1) {
            if (how === 0) {
                sorter = titleAsc
            } else {
                sorter = titleDes
            }
        } else if (what === 2) {
            if (how === 0) {
                sorter = commentAsc
            } else {
                sorter = commentDes
            }
        } else if (what === 3) {
            if (how === 0) {
                sorter = lengthAsc
            } else {
                sorter = lengthDes
            }
        } else if (what === 4) {
            if (how === 0) {
                sorter = genreAsc
            } else {
                sorter = genreDes
            }
        } else if (what === 5) {
            if (how === 0) {
                sorter = artistAsc
            } else {
                sorter = artistDes
            }
        } else {
            console.error("sort criterion is wrong")
            return
        }

        for (var i = 0; i < playlist_model.count; ++i) {
            if (i < 1) {
                continue
            }
            if (i === 1) {
                // if 0 is not < 1 move 0 to 1
                if (sorter(1, 0)) {
                    playlist_model.move(0, 1, 1)
                }
                continue
            }
            if (sorter(i, i - 1)) {
                playlist_model.move(i - 1, i, 1)
                i -= 2
            }
        }

        updateNowPlayingRow()
    }

    // returns true if at i < j
    function trackAsc(i, j) {
        return parseInt(playlist_model.get(i).track) < parseInt(
                    playlist_model.get(j).track)
    }
    function titleAsc(i, j) {
        return playlist_model.get(i).title < playlist_model.get(j).title
    }
    function commentAsc(i, j) {
        return playlist_model.get(i).comment < playlist_model.get(j).comment
    }
    function lengthAsc(i, j) {
        return playlist_model.get(i).length < playlist_model.get(j).length
    }
    function genreAsc(i, j) {
        return playlist_model.get(i).genre < playlist_model.get(j).genre
    }
    function artistAsc(i, j) {
        return playlist_model.get(i).artist < playlist_model.get(j).artist
    }
    // returns true if at i > j
    // I implemented the descending functions. Just negating the
    // ascending functions doesn't work. That would contain the equal.
    // Having equal doesn't work.
    function trackDes(i, j) {
        return parseInt(playlist_model.get(i).track) > parseInt(
                    playlist_model.get(j).track)
    }
    function titleDes(i, j) {
        return playlist_model.get(i).title > playlist_model.get(j).title
    }
    function commentDes(i, j) {
        return playlist_model.get(i).comment > playlist_model.get(j).comment
    }
    function lengthDes(i, j) {
        return playlist_model.get(i).length > playlist_model.get(j).length
    }
    function genreDes(i, j) {
        return playlist_model.get(i).genre > playlist_model.get(j).genre
    }
    function artistDes(i, j) {
        return playlist_model.get(i).artist > playlist_model.get(j).artist
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

    ListModel {
        id: playlist_model
    }

    TableView {
        id: playlist_view
        anchors.fill: parent
        model: playlist_model
        onCurrentRowChanged: {
            selection.clear()
            selection.select(currentRow)
        }
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

        onModelChanged: playlist_view.resizeColumnsToContents()

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
    }
}
