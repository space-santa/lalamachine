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

Row {
    id: btn_row

    width: childrenRect.width
    height: childrenRect.height

    ImageButton {
        id: back_btn
        action: back_action
        source: "../images/back.png"
    }

    ImageButton {
        id: play_btn
        action: play_pause_action
        source: playMusic.isPlaying ? "../images/pause.png" : "../images/play.png"
    }

    ImageButton {
        id: open_btn
        action: open_action
        source: "../images/eject.png"
    }

    ImageButton {
        id: next_btn
        action: forward_action
        source: "../images/forward.png"
    }
}
