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
import QtQuick.Layouts 1.1
import QtQuick.Controls 1.2

Item {
    height: 40

    property alias andorText: andorbox.currentText
    property alias whereText: wherebox.currentText
    property alias howText: howbox.currentText
    property alias valueText: value_field.text
    property bool showAndOr: true

    function init() {
        andorbox.currentIndex = 0
        wherebox.currentIndex = 0
        howbox.currentIndex = 0
        value_field.text = ""
    }

    function set(andor, where, how, value) {
        andorbox.currentIndex = andorbox.find(translate(andor))
        wherebox.currentIndex = wherebox.find(translate(where))
        howbox.currentIndex = howbox.find(translate(how))
        value_field.text = value
    }

    function translate(what) {
        if (what === "AND") {
            return "and"
        }
        if (what === "OR") {
            return "or"
        }

        if (what === "LIKE") {
            return "contains"
        }
        if (what === "NOT LIKE") {
            return "contains not"
        }
        if (what === "IS") {
            return "is"
        }
        if (what === "IS NOT") {
            return "is not"
        }

        if (what === "genre") {
            return "Genre"
        }
        if (what === "artist") {
            return "Artist"
        }
        if (what === "album") {
            return "Album"
        }
        if (what === "comment") {
            return "Comment"
        }
        if (what === "title") {
            return "Title"
        }
    }

    Item {
        id: grid
        anchors.top: parent.top
        anchors.left: parent.left
        anchors.bottom: parent.bottom

        width: 385
        height: 40

        ComboBox {
            id: andorbox
            anchors.top: parent.top
            anchors.left: parent.left
            anchors.margins: 5
            width: 120
            height: 30
            model: ["and", "or"]
            visible: showAndOr
        }

        ComboBox {
            id: wherebox
            anchors.top: parent.top
            anchors.left: andorbox.right
            anchors.margins: 5
            width: 120
            height: 30
            model: ["Genre", "Artist", "Album", "Comment", "Title"]
        }

        ComboBox {
            id: howbox
            anchors.top: parent.top
            anchors.left: wherebox.right
            anchors.margins: 5
            width: 120
            height: 30
            model: ["contains", "contains not", "is", "is not"]
        }
    }

    TextField {
        id: value_field
        anchors.left: grid.right
        anchors.top: parent.top
        anchors.right: parent.right
        anchors.margins: 5

        height: 30
    }
}
