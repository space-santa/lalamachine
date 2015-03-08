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
import QtQuick.Controls.Styles 1.2

Button {
    width: 70
    height: width
    property string source
    property string colourUp: "transparent"
    property string colourDown: "lightgrey"

    Image {
        anchors.centerIn: parent
        width: parent.width - 10
        height: parent.height - 10
        source: parent.source
    }
    style: ButtonStyle {
        background: Rectangle {
            anchors.fill: parent
            color: checkable ? (checked ? colourDown : colourUp)
                             : (control.pressed ? colourDown : colourUp)
        }
    }
}
