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
import QtQuick.Dialogs 1.2

Dialog {
    id: savePlaylistDialog
    visible: false
    title: "Choose a name"
    property string playlistName: name_field.text
    standardButtons: StandardButton.Save | StandardButton.Cancel

    onAccepted: console.log("Saving the date " + name_field.text)
    onRejected: {
        visible = false
        name_field.text = ""
    }

    Column {
        anchors.top: parent.top
        anchors.left: parent.left
        anchors.right: parent.right

        spacing: 5

        Text {
            width: parent.width
            text: "Playlist name:"
        }

        TextField {
            id: name_field
            width: parent.width
            validator: RegExpValidator {
                regExp: /^(?!cs1m090$).*/
            }
        }
    }
}
