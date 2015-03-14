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
import QtQuick.Layouts 1.1

import Lala 1.0

Rectangle {
    color: "transparent"

    property bool scanInProgress: lib.scanning

    signal addTrack(string path)

    function rescan() {
        lib.rescan()
    }

    MusicLib {
        id: lib
        libPath: "/home/rmean/musiclib"

        genreFilter: genreList.selection
        artistFilter: artistList.selection
        albumFilter: albumList.selection

        onDisplayLibChanged: {
            bottomShelve.clearList()
            bottomShelve.addLib(displayLib)
        }
    }

    Rectangle {
        id: topShelve
        anchors.top: parent.top
        anchors.left: parent.left
        anchors.right: parent.right
        height: parent.height / 2

        color: "transparent"

        StringListView {
            id: genreList
            anchors.top: parent.top
            anchors.bottom: parent.bottom
            anchors.left: parent.left
            width: parent.width / 3
            allowSort: false
            roleString: "genre"
            stringList: lib.genreList
        }
        StringListView {
            id: artistList
            anchors.top: parent.top
            anchors.bottom: parent.bottom
            anchors.left: genreList.right
            width: parent.width / 3
            height: parent.height
            allowSort: false
            roleString: "artist"
            stringList: lib.artistList
        }
        StringListView {
            id: albumList
            anchors.top: parent.top
            anchors.bottom: parent.bottom
            anchors.right: parent.right
            width: parent.width / 3
            allowSort: false
            roleString: "album"
            stringList: lib.albumList
        }
    }

    Playlist {
        id: bottomShelve
        anchors.top: topShelve.bottom
        anchors.left: parent.left
        anchors.right: parent.right
        anchors.bottom: parent.bottom

        color: "transparent"

        provideTotalTime: false

        onPlay: addTrack(path)
    }
}
