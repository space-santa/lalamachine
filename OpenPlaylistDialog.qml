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
    width: 400
    height: 600

    visible: false
    standardButtons: StandardButton.Open | StandardButton.Cancel

    property string selection: {
        namelist_model.get(namelist_view.currentRow) ?
                    namelist_model.get(namelist_view.currentRow).listname : ""
    }

    onSelectionChanged: console.log(selection)

    onAccepted: {
        close()
    }
    onRejected: {
        selection = ""
        close()
    }
    function close() {
        clearList()
        visible = false
    }

    function addList(list) {
        for (var i = 0; i < list.length; ++i) {
            add(list[i].toString())
        }
    }

    function add(listname) {
        namelist_model.append({
                                  listname: listname
                              })
    }

    function clearList() {
        namelist_model.clear()
    }

    Rectangle {
        anchors.top: parent.top
        anchors.left: parent.left
        anchors.right: parent.right
        height: 500

        ListModel {
            id: namelist_model
        }

        TableView {
            id: namelist_view

            anchors.top: parent.top
            anchors.left: parent.left
            anchors.right: parent.right
            anchors.bottom: parent.bottom

            model: namelist_model
            onCurrentRowChanged: {
                selection.clear()
                selection.select(currentRow)
            }
            TableViewColumn {
                role: "listname"
                title: "name"
                width: parent.width
            }

            onDoubleClicked: {
                accepted()
            }
        }
    }
}
