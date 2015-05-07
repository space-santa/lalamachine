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
    property alias library: lib.libPath
    property alias musicLib: lib

    signal addTrack(string path)

    function rescan() {
        lib.rescan()
    }

    MusicLib {
        id: lib

        genreFilter: genreList.selection
        artistFilter: artistList.selection
        albumFilter: albumList.selection

        titlePartialFilter: filter_text.text

        what: titles.sortwhat
        sortAsc: titles.sortAsc

        onDisplayLibChanged: {
            titles.clearList()
            titles.addLib(displayLib)
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

    Rectangle {
        id: bottomShelve
        anchors.top: topShelve.bottom
        anchors.left: parent.left
        anchors.right: parent.right
        anchors.bottom: parent.bottom
        color: "transparent"

        Rectangle {
            id: tiles_container
            anchors.top: parent.top
            anchors.left: parent.left
            anchors.right: parent.right
            anchors.bottom: scan_notifier.top
            color: "transparent"

            Item {
                id: titles_filter_box
                anchors.top: parent.top
                anchors.horizontalCenter: titles.horizontalCenter
                height: 25
                width: 250

                TextField {
                    id: filter_text
                    anchors.left: parent.left
                    anchors.leftMargin: 5
                    anchors.top: parent.top
                    anchors.bottom: parent.bottom
                    anchors.right: clear_button.left

                    placeholderText: "Filter..."

                    onTextChanged: key_events.restartTimer()

                    MouseArea {
                        id: input_grabber
                        anchors.fill: parent
                        enabled: key_events.keepFocus

                        onClicked: {
                            key_events.keepFocus = false
                            filter_text.forceActiveFocus()
                        }
                    }
                }

                ImageButton {
                    id: clear_button
                    width: height
                    anchors.right: parent.right
                    anchors.rightMargin: 5
                    anchors.top: parent.top
                    anchors.bottom: parent.bottom

                    source: "qrc:/images/images/edit-clear.png"

                    onClicked: {
                        var ts = Date.now()
                        // We don't want to setDisplayLib here...
                        lib.canSetDisplayLib = false
                        lib.resetFilterAndSort()
                        genreList.reset()
                        artistList.reset()
                        albumList.reset()
                        lib.canSetDisplayLib = true
                        // because when setting the filter_text.text will also
                        // setDisplayLib.
                        filter_text.text = ""
                        console.log("Reset filter duration:", Date.now() - ts)
                    }
                }
            }

            Playlist {
                id: titles
                isLibrary: true
                anchors.top: titles_filter_box.bottom
                anchors.left: parent.left
                anchors.right: parent.right
                anchors.bottom: titles_info.top

                color: "transparent"

                onPlay: lalaplayer.playTrack(path)
            }

            Rectangle {
                id: titles_info
                anchors.left: parent.left
                anchors.right: parent.right
                anchors.bottom: parent.bottom
                height: 20
                color: "transparent"

                Text {
                    anchors.fill: parent

                    visible: true

                    text: titles.count + " Tracks, Total length: " + titles.totalPlaytimeString
                    color: "#ffffff"
                    verticalAlignment: Text.AlignVCenter
                    horizontalAlignment: Text.AlignHCenter
                    font.pointSize: 12
                    styleColor: "#000000"
                    style: Text.Outline
                }
            }
        }

        Rectangle {
            id: scan_notifier
            anchors.left: parent.left
            anchors.right: parent.right
            anchors.bottom: parent.bottom
            height: lib.scanning ? 50 : 0
            color: "transparent"

            Text {
                anchors.fill: parent

                visible: lib.scanning

                text: "Scan in progress..."
                color: "white"
                font.pointSize: 16
                verticalAlignment: Text.AlignVCenter
                horizontalAlignment: Text.AlignHCenter
            }
        }
    }
}
