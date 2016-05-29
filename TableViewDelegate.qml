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
import QtQuick.Controls 1.3

Item {
    id: container
    property TableView target
    signal rightClick(int row)
    signal pressed(int row)
    signal released
    signal leftClicked(int row)
    signal doubleClicked(int row)
    // This signal must communicate more than just the y for drag and move to work.
    // We need the height to determine when y has moved enough to get into a new row.
    // And we need the row of this as a starting point for future row movement.
    // This works, because as long as the mouse remains pressed (as long
    // as the user is dragging and moving) row is the row where the first click was
    // and y is relative to the first click.
    signal mouseYChanged(int y, int height, int row)

    Rectangle {
        anchors {
            left: parent.left
            right: parent.right
            verticalCenter: parent.verticalCenter
        }
        height: parent.height
        color: styleData.selected ? 'darkslategrey' : "transparent"
        MouseArea {
            anchors.fill: parent
            acceptedButtons: Qt.RightButton | Qt.LeftButton
            onClicked: {
                if (mouse.button == Qt.RightButton) {
                    console.log("Right-Click", styleData.row)
                    if (target.selection.count < 2) {
                        target.selection.clear()
                        target.selection.select(styleData.row)
                    }
                    rightClick(styleData.row)
                }
            }

            onDoubleClicked: container.doubleClicked(styleData.row)

            onPressed: {
                if (mouse.button == Qt.LeftButton) {
                    // For drag and click to work properly we need to first
                    // set the state to released when clicking into a row.
                    // This is to be sure that no drag is happening until we
                    // finished the preparations.
                    container.released()
                    // Next we set the mouseY position to start with. This must
                    // happen before we set the pressed flag to aboid accidental
                    // 'jumping' which could happen if the pressed flag is set
                    // and a new mouseY is set. The view could assume that a
                    // drag happend because the Y coordinate changed.
                    emitMouseYChanged()
                    // All preps are finished, set the pressed flag.
                    container.pressed(styleData.row)

                    target["clicked"](styleData.row)
                    container.leftClicked(styleData.row)
                }
            }

            // We also want to emit the released signal when it actually
            // happens.
            onReleased: container.released()

            onMouseYChanged: {
                // WARNING: Only do this for the left button. Else opening
                // the right click menu might move tracks around.
                if (pressedButtons === Qt.LeftButton) {
                    emitMouseYChanged()
                }
            }

            function emitMouseYChanged() {
                    container.mouseYChanged(mouseY, container.height, styleData.row)
            }
        }
    }
}
