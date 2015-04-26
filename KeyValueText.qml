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

Rectangle {
    width: 200
    height: 40

    color: "transparent"

    property int labelWidth: 60
    property alias labelText: label.text

    property alias valueText: value.text

    Text {
        id: label
        width: labelWidth
        anchors.left: parent.left
        anchors.top: parent.top
        anchors.bottom: parent.bottom

        verticalAlignment: Qt.AlignVCenter
        horizontalAlignment: Qt.AlignRight
    }

    Text {
        id: value
        anchors.right: parent.right
        anchors.top: parent.top
        anchors.bottom: parent.bottom
        anchors.left: label.right

        verticalAlignment: Qt.AlignVCenter
        horizontalAlignment: Qt.AlignLeft
    }
}
