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

Column {
    height: childrenRect.height
    width: childrenRect.width
    signal openList
    signal saveList
    signal moveTop
    signal moveUp
    signal moveDown
    signal moveBottom
    signal clearList
    signal deleteList
    signal burnList

    ImageButton {
        id: save_btn
        width: 50
        source: "qrc:/images/images/save.png"
        onClicked: saveList()
    }
    ImageButton {
        id: top_btn
        width: 50
        source: "qrc:/images/images/go-top.png"
        onClicked: moveTop()
    }
    ImageButton {
        id: up_btn
        width: 50
        source: "qrc:/images/images/go-up.png"
        onClicked: moveUp()
    }
    ImageButton {
        id: down_btn
        width: 50
        source: "qrc:/images/images/go-down.png"
        onClicked: moveDown()
    }
    ImageButton {
        id: bottom_btn
        width: 50
        source: "qrc:/images/images/go-bottom.png"
        onClicked: moveBottom()
    }
    ImageButton {
        id: clear_btn
        width: 50
        source: "qrc:/images/images/edit-clear.png"
        onClicked: clearList()
    }
}
