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
    property alias keyTitle: key_column.title
    property alias valueTitle: value_column.title

    signal rightClick

    // The list will contain the JsonArray.
    // Each JSON object must have a "key" and a "value".
    property var list

    onListChanged: {
        updateList()
    }

    Component.onCompleted: updateList()

    function updateList() {
        model.clear()

        if (list) {
            for (var i = 0; i < list.length; ++i) {
                model.append(list[i])
            }
        }
    }

    function getJson() {
        var retval = []
        if (model && model.count > 0) {
            for (var i = 0; i < model.count; ++i) {
                if (model.get(i)) {
                    retval.push({
                                    key: model.get(i).key,
                                    value: model.get(i).value
                                })
                }
            }
        }
        return retval
    }

    function moveUp() {
        model.move(view.currentRow, view.currentRow - 1, 1)
        view.selection.clear()
        view.selection.select(view.currentRow - 1)
    }

    function moveDown() {
        model.move(view.currentRow, view.currentRow + 1, 1)
        view.selection.clear()
        view.selection.select(view.currentRow + 1)
    }

    function setSelectionEnabled(val) {
        // DANGER! WARNING! DRAGONS RIGHT HERE!
        // currentRow doesn't work on right click because the delegate is
        // setting the selection. That does not affect the currentRow. Hence we
        // have to go through the selections. But that has the
        // (actually desired) side effect that multiple things can be changed
        // at once.
        view.selection.forEach(function (rowIndex) {
            model.setProperty(rowIndex, "value", val)
        })
    }

    ListModel {
        id: model
    }

    TableView {
        id: view
        anchors.fill: parent
        selectionMode: SelectionMode.ContiguousSelection

        model: model

        TableViewColumn {
            id: key_column
            width: (container.width - 2) / 2
            role: "key"
        }

        TableViewColumn {
            id: value_column
            width: (container.width - 2) / 2
            role: "value"
        }

        rowDelegate: TableViewDelegate {
            target: view
            onRightClick: container.rightClick()
        }
    }
}
