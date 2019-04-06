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
    id: container
    color: "transparent"

    property bool scanInProgress: lib.scanning
    property string library: config.libPath
    property alias musicLib: lib

    signal addTrack(string path)

    LibSettings {
        id: libsettings
    }

    function rescan(path) {
        if (!libview.scanInProgress) {
            lib.rescan(path)
        }
    }

    function playNext(random) {
        titles.playNext(random)
    }

    function playPrevious() {
        titles.playPrevious()
    }

    function playCurrentTrack() {
        titles.playCurrentTrack()
    }

    MusicLib {
        id: lib

        //titlePartialFilter: filter_text.text

        //sortAsc: titles.sortAsc

        function updateDisplayLib() {
            titles.emptyCurrentList()
            titles.addLib(JSON.parse(displayLib))
        }

        onScanDone: function() {
            console.log("message: scan done")
            scan_notifier.hide()
            topShelve.updateLists()
        }

        function rescan(path) {
            scan_notifier.show();
            scanAsync(path)
        }
    }

    SplitView {
        anchors.fill: parent
        orientation: Qt.Vertical

        Rectangle {
            id: topShelve
            anchors.top: parent.top
            anchors.left: parent.left
            anchors.right: parent.right
            height: 250

            Layout.minimumHeight: 200

            color: "transparent"

            onHeightChanged: {
                if (libsettings.isActive) {
                    libsettings.libraryTopShelveHeight = height
                }
                libsettings.isActive = true;
            }

            Component.onCompleted: {
                if (libsettings.libraryTopShelveHeight > 0) {
                    height = libsettings.libraryTopShelveHeight
                }
                libsettings.isActive = true
                updateLists()
            }

            function updateLists() {
                genre_list.stringList = JSON.parse(lib.genreList)
                album_list.stringList = JSON.parse(lib.albumList)
                artist_list.stringList = JSON.parse(lib.artistList)
                lib.updateDisplayLib()
            }

            StringListView {
                id: genre_list
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                anchors.left: parent.left
                width: parent.width / 3
                allowSort: false
                roleString: "genre"
                stringList: lib.genreList

                onSelectionChanged: function() {
                    lib.genreFilter = selection;
                    artist_list.stringList = JSON.parse(lib.artistList)
                    album_list.stringList = JSON.parse(lib.albumList)
                    lib.updateDisplayLib()
                }
            }
            StringListView {
                id: artist_list
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                anchors.left: genre_list.right
                width: parent.width / 3
                height: parent.height
                allowSort: false
                roleString: "artist"
                stringList: lib.artistList

                onSelectionChanged: function() {
                    lib.artistFilter = selection;
                    album_list.stringList = JSON.parse(lib.albumList)
                    lib.updateDisplayLib()
                }
            }
            StringListView {
                id: album_list
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                anchors.right: parent.right
                width: parent.width / 3
                allowSort: false
                roleString: "album"
                stringList: lib.albumList

                onSelectionChanged: function() {
                    lib.albumFilter = selection;
                    lib.updateDisplayLib()
                }

                onRightClick: {
                    rcm.popup()
                }
            }

            LibraryPlaylistRightClickMenu {
                id: rcm

                onAddToPlaylist: {
                    if (listname === "") {
                        mainPlaylist.createNewList()
                        listname = miscPlaylistName
                    }

                    var albumList = JSON.parse(musicLib.getAlbumTracks(album_list.selection))

                    for (var i = 0; i < albumList.length; ++i) {
                        mainPlaylist.add(albumList[i].path)
                    }

                    mainPlaylist.updateAndSave()
                }
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
                    height: 32
                    width: 250

                    TextField {
                        id: filter_text
                        anchors.left: parent.left
                        anchors.leftMargin: 5
                        anchors.top: parent.top
                        anchors.bottom: parent.bottom
                        anchors.right: clear_button.left

                        font.pointSize: 12
                        font.family: "Helvetica"

                        placeholderText: "Filter..."

                        onTextChanged: function() {
                            if (text.length > 2 || text.length < 1) {
                                lib.searchString = text
                                topShelve.updateLists()
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

                        source: "../images/edit-clear.png"

                        onClicked: {
                            // Force onTextChanged if the search box is empty.
                            // This is to reset the genre / artist / album filter.
                            filter_text.text = " "
                            filter_text.text = ""
                        }
                    }
                }

                LibraryPlaylist {
                    id: titles
                    isLibrary: true
                    anchors.top: titles_filter_box.bottom
                    anchors.left: parent.left
                    anchors.right: parent.right
                    anchors.bottom: titles_info.top

                    color: "transparent"

                    onAddTracksToNamedPlaylist: m3u.addTracksToNamedPlaylist(
                                                    listname, JSON.stringify(tracks))

                    onPlay: lalaplayer.playTrack(path, title, artist)
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
                        font.family: "Helvetica"
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
                height: 0
                color: "transparent"

                function show() {
                    height = 50;
                    now_scanning_text.visible = true
                }

                function hide() {
                    height = 0;
                    now_scanning_text.visible = false
                }

                TextBase {
                    id: now_scanning_text
                    anchors.fill: parent

                    visible: false

                    text: "Scan in progress..."
                    color: "white"
                    font.pointSize: 16
                    verticalAlignment: Text.AlignVCenter
                    horizontalAlignment: Text.AlignHCenter
                }
            }
        }
    }
}
