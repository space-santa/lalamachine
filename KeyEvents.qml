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

Item {
    focus: true

    signal volumeUp
    signal volumeDown
    signal forward
    signal back
    signal pause
    signal deleteTrack
    signal startPlaying

    // This is to always get the focus back. Otherwise e.g. the playlist View
    // will keep the focus.
    onFocusChanged: focus = true

    Keys.onUpPressed: {
        volumeUp()
    }

    Keys.onDownPressed: {
        volumeDown()
    }

    Keys.onLeftPressed: {
        back()
    }

    Keys.onRightPressed: {
        forward()
    }

    Keys.onSpacePressed: {
        pause()
    }

    Keys.onDeletePressed: {
        deleteTrack()
    }

    Keys.onEnterPressed: {
        console.log("playing current row")
        startPlaying()
    }
}
