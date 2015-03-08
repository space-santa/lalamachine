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

import Lala 1.0

Rectangle {
    id: container
    color: "transparent"

    property bool allowSort: true

    property string roleString: ""
    property var stringList: []

    property string selection: getSelection()

    onStringListChanged: {
        list_model.clear()

        for (var i = 0; i < stringList.length; ++i) {
            list_model.append({
                                  content: stringList[i]
                              })
        }
    }

    function getSelection() {
        if (list_model.get(list_view.currentRow)) {
            return list_model.get(list_view.currentRow).content
        } else {
            return ""
        }
    }

    function sort(how) {
        if (!container.allowSort) {
            return
        }

        var sorter
        if (how === 0) {
            sorter = contentAsc
        } else {
            sorter = contentDes
        }

        for (var i = 0; i < list_model.count; ++i) {
            if (i < 1) {
                continue
            }
            if (i === 1) {
                // if 0 is not < 1 move 0 to 1
                if (sorter(1, 0)) {
                    list_model.move(0, 1, 1)
                }
                continue
            }
            if (sorter(i, i - 1)) {
                list_model.move(i - 1, i, 1)
                i -= 2
            }
        }
    }
    function contentAsc(i, j) {
        return list_model.get(i).content < list_model.get(j).content
    }
    function contentDes(i, j) {
        return list_model.get(i).content > list_model.get(j).content
    }

    ListModel {
        id: list_model
    }

    TableView {
        id: list_view
        anchors.fill: parent
        model: list_model
        onCurrentRowChanged: {
            selection.clear()
            selection.select(currentRow)
        }
        TableViewColumn {
            role: "content"
            title: container.roleString
            width: container.width
        }

        onModelChanged: list_view.resizeColumnsToContents()

        sortIndicatorVisible: container.allowSort
        onSortIndicatorColumnChanged: {
            sort(sortIndicatorOrder)
        }
        onSortIndicatorOrderChanged: {
            sort(sortIndicatorOrder)
        }
    }
}
