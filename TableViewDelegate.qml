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

Item {
    id: container
    property TableView target
    signal rightClick(int row)
    signal pressed(int row)
    signal released
    signal leftClicked(int row)
    signal doubleClicked(int row)
    signal mouseYChanged(int y, int baseY)

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
                    target["clicked"](styleData.row)
                    container.pressed(styleData.row)
                    container.leftClicked(styleData.row)
                }
            }

            onReleased: container.released()

            onMouseYChanged: {
                // WARNING: Only do this for the left button. Else opening
                // the right click menu might move tracks around.
                if (pressedButtons === Qt.LeftButton) {
                    container.mouseYChanged(mouseY,
                                            styleData.row * container.height)
                }
            }
        }
    }
}
