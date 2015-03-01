import QtQuick 2.0
import QtQuick.Controls 1.2
import QtQuick.Layouts 1.1

import Lala 1.0

Rectangle {
    color: "transparent"

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
            roleString: "artist"
            stringList: lib.artistList
        }
        StringListView {
            id: albumList
            anchors.top: parent.top
            anchors.bottom: parent.bottom
            anchors.right: parent.right
            width: parent.width / 3
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
    }
}
