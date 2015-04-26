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
import QtQuick.Controls 1.3

Item {
    height: 75

    property alias andorText: andorbox.currentText
    property alias whereText: wherebox.currentText
    property alias howText: howbox.currentText
    property alias valueText: value_field.text

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
        height: 75

        Text {
            id: andor_label
            anchors.top: parent.top
            anchors.left: parent.left
            anchors.margins: 5
            width: 120
            height: 30
            text: "How to apply?"
        }
        ComboBox {
            id: andorbox
            anchors.top: andor_label.bottom
            anchors.left: parent.left
            anchors.margins: 5
            width: 120
            height: 30
            model: ["and", "or"]
        }

        Text {
            id: where_label
            anchors.top: parent.top
            anchors.left: andor_label.right
            anchors.margins: 5
            width: 120
            height: 30
            text: "Where?"
        }
        ComboBox {
            id: wherebox
            anchors.top: where_label.bottom
            anchors.left: andorbox.right
            anchors.margins: 5
            width: 120
            height: 30
            model: ["Genre", "Artist", "Album", "Comment", "Title"]
        }

        Text {
            id: how_label
            anchors.top: parent.top
            anchors.left: where_label.right
            anchors.margins: 5
            width: 120
            height: 30
            text: "How?"
        }
        ComboBox {
            id: howbox
            anchors.top: how_label.bottom
            anchors.left: wherebox.right
            anchors.margins: 5
            width: 120
            height: 30
            model: ["contains", "contains not", "is", "is not"]
        }
    }

    Text {
        id: value_label
        anchors.left: grid.right
        anchors.top: parent.top
        anchors.right: parent.right
        anchors.margins: 5

        height: 30
        text: "Value?"
    }
    TextField {
        id: value_field
        anchors.left: grid.right
        anchors.top: value_label.bottom
        anchors.right: parent.right
        anchors.margins: 5

        height: 30
    }
}
