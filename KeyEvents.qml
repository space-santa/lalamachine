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

    property bool keepFocus: true

    onKeepFocusChanged: focus_timer.restart()

    // This is to always get the focus back. Otherwise e.g. the playlist View
    // will keep the focus.
    // This must change because I need to be able to type into the filter
    // text input fields which requires focus.
    // The textinputs will emit a signal to get focus.
    // Once they are done focus gets back. How todetermine when they are done?
    onFocusChanged: {
        if (keepFocus) {
            focus = true
        }
    }

    Timer {
        id: focus_timer
        repeat: false
        running: false
        interval: 5000
        onTriggered: {
            keepFocus = true
            focus = true
        }
    }

    function restartTimer() {
        focus_timer.restart()
    }

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
