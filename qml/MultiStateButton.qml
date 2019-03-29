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

ImageButton {
    id: repeat_btn
    width: 50
    source: imageSource()
    checkable: true
    checked: none ? false : true

    property bool none: true
    property bool repeatAll: false
    property bool repeatOne: false
    property bool random: false

    signal selectionChanged

    onSelectionChanged: checkSelection()

    onNoneChanged: {
        if (none) {
            console.log("none")
            selectionChanged()
        }
    }
    onRepeatAllChanged: {
        if (repeatAll) {
            console.log("repeatAll")
            selectionChanged()
        }
    }
    onRepeatOneChanged: {
        if (repeatOne) {
            console.log("repeatOne")
            selectionChanged()
        }
    }
    onRandomChanged: {
        if (random) {
            console.log("random")
            selectionChanged()
        }
    }

    function checkSelection() {
        if (none) {
            repeatAll = false
            repeatOne = false
            random = false
        }
        if (repeatAll) {
            none = false
            repeatOne = false
            random = false
        }
        if (repeatOne) {
            none = false
            repeatAll = false
            random = false
        }
        if (random) {
            repeatAll = false
            repeatOne = false
            none = false
        }
    }

    function imageSource() {
        if (repeatOne) {
            return "../images/repeat1.png"
        }
        if (repeatAll) {
            return "../images/repeatAll.png"
        }
        if (random) {
            return "../images/random.png"
        }
        return "../images/repeat_random.png"
    }

    MouseArea {
        anchors.fill: parent

        onClicked: {
            if (none) {
                none = false
                repeatAll = true
            } else if (repeatAll) {
                repeatAll = false
                repeatOne = true
            } else if (repeatOne) {
                repeatOne = false
                random = true
            } else {
                random = false
                none = true
            }
        }
    }
}
