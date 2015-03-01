import QtQuick 2.0
import QtQuick.Controls 1.2
import QtQuick.Layouts 1.1

import Lala 1.0

Rectangle {
    width: 100
    height: 62

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

        RowLayout {
            anchors.fill: parent
            StringListView {
                id: genreList
                width: parent.width / 3
                height: parent.height
                roleString: "genre"
                stringList: lib.genreList
            }
            StringListView {
                id: artistList
                width: parent.width / 3
                height: parent.height
                roleString: "artist"
                stringList: lib.artistList
            }
            StringListView {
                id: albumList
                width: parent.width / 3
                height: parent.height
                roleString: "album"
                stringList: lib.albumList
            }
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
