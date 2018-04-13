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
import QtQuick.Dialogs 1.2
import QtQuick.Layouts 1.1
import QtQuick.Controls 1.2

Dialog {
    id: container
    title: "Automatic Playlist"
    width: 600
    height: 270
    standardButtons: StandardButton.Ok | StandardButton.Cancel

    property var values
    property alias apName: name_field.text

    function collectValues() {
        var retval = []
        retval[0] = {
            // The first line is always "and".
            andor: "and",
            tag: apo1.whereText,
            operator: apo1.howText,
            value: apo1.valueText
        }

        if (apo2.visible) {
            retval[1] = {
                andor: apo2.andorText,
                tag: apo2.whereText,
                operator: apo2.howText,
                value: apo2.valueText
            }
        }

        if (apo3.visible) {
            retval[2] = {
                andor: apo3.andorText,
                tag: apo3.whereText,
                operator: apo3.howText,
                value: apo3.valueText
            }
        }

        return retval
    }

    function setApo1(andor, where, how, value) {
        apo1.set(andor, where, how, value)
    }
    function setApo2(andor, where, how, value) {
        apo2.set(andor, where, how, value)
        apo2.visible = true
    }
    function setApo3(andor, where, how, value) {
        apo3.set(andor, where, how, value)
        apo3.visible = true
    }

    function clearAll() {
        apo1.init()
        apo2.init()
        apo3.init()
        apo2.visible = false
        apo3.visible = false
    }

    function canQuit() {
        return (name_field.text !== "" && apo1.valueText !== "")
    }

    function createNew() {
        open()
        clearAll()
    }

    function editAutoPlaylist(listname, autoPlaylist) {
        clearAll()

        setApo1(autoPlaylist[0].andor, autoPlaylist[0].tag, autoPlaylist[0].operator,
                    arr[0].value)

        if (autoPlaylist[1]) {
            setApo2(autoPlaylist[1].andor, autoPlaylist[1].tag, autoPlaylist[1].operator, autoPlaylist[1].value)
        }

        if (autoPlaylist[2]) {
            setApo3(autoPlaylist[2].andor, autoPlaylist[2].tag, autoPlaylist[2].operator, autoPlaylist[2].value)
        }

        apName = listname
        open()
    }

    onRejected: {
        clearAll()
        close()
    }

    Rectangle {
        anchors.fill: parent
        color: "transparent"

        TextField {
            id: name_field

            anchors.top: parent.top
            anchors.horizontalCenter: parent.horizontalCenter
            anchors.margins: 5

            width: parent.width / 2
            height: 35

            placeholderText: "playlist name"
        }

        AutoPlaylistObject {
            id: apo1

            anchors.top: name_field.bottom
            anchors.topMargin: 10
            anchors.left: parent.left

            width: parent.width
            height: 40

            showAndOr: false

            onVisibleChanged: {
                if (!visible) {
                    apo1.init()
                }
            }
        }

        Item {
            id: buttons1

            anchors.top: apo1.bottom
            anchors.left: parent.left

            height: apo2.visible ? 0 : 50
            width: parent.width

            visible: apo2.visible ? false : true

            ImageButton {
                id: add_button1
                anchors.left: parent.left
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                width: 50
                source: "qrc:/images/images/list-add.png"

                onClicked: {
                    apo2.visible = true
                }
            }
        }

        AutoPlaylistObject {
            id: apo2

            anchors.top: buttons1.bottom
            anchors.left: parent.left

            width: parent.width
            height: 40

            visible: false

            onVisibleChanged: {
                if (!visible) {
                    apo2.init()
                }
            }
        }

        Item {
            id: buttons2

            anchors.top: apo2.bottom
            anchors.left: parent.left

            height: apo3.visible ? 0 : 50
            width: parent.width
            visible: setVisible()

            function setVisible() {
                return apo2.visible && !apo3.visible
            }

            ImageButton {
                id: add_button2
                anchors.left: parent.left
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                width: 50
                source: "qrc:/images/images/list-add.png"

                onClicked: {
                    apo3.visible = true
                }
            }
            ImageButton {
                id: del_button2
                anchors.left: add_button2.right
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                width: 50
                source: "qrc:/images/images/list-remove.png"

                onClicked: {
                    buttons1.visible = true
                    apo2.visible = false
                }
            }
        }

        AutoPlaylistObject {
            id: apo3

            anchors.top: buttons2.bottom
            anchors.left: parent.left

            width: parent.width
            height: 40

            visible: false

            onVisibleChanged: {
                if (!visible) {
                    apo3.init()
                }
            }
        }

        Item {
            id: buttons3

            anchors.top: apo3.bottom
            anchors.left: parent.left

            height: 50
            width: parent.width
            visible: apo3.visible ? true : false
            ImageButton {
                id: del_button3
                anchors.left: parent.left
                anchors.top: parent.top
                anchors.bottom: parent.bottom
                width: 50
                source: "qrc:/images/images/list-remove.png"

                onClicked: {
                    apo3.visible = false
                }
            }
        }
    }
}
