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

import Lala 1.0

Rectangle {
    id: container
    color: "transparent"

    property bool allowSort: true

    property string roleString: ""
    property var stringList: []

    property string selection: getSelection()

    signal rightClick

    onStringListChanged: {
        list_model.clear()

        for (var i = 0; i < stringList.length; ++i) {
            list_model.append({
                                  content: stringList[i]
                              })
        }
    }

    Component.onCompleted: reset()

    function reset() {
        list_view.selection.select(0)
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

    Rectangle {
        anchors.fill: parent
        color: "transparent"

        TableView {
            id: list_view
            anchors.top: info.bottom
            anchors.left: parent.left
            anchors.right: parent.right
            anchors.bottom: parent.bottom
            model: list_model
            backgroundVisible: false

            // I found the horizontal scrollbars annoying for this lists.
            // They take up too much space and don't really give better info
            // when searching through e.g. a list of genres.
            horizontalScrollBarPolicy: Qt.ScrollBarAlwaysOff

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

            onClicked: {
                currentRow = row
            }

            rowDelegate: TableViewDelegate {
                target: list_view
                onRightClick: {
                    container.rightClick()
                    list_view.currentRow = row
                }
            }

            itemDelegate: ItemDelegate {
                Text {
                    color: "white"
                    elide: styleData.elideMode
                    text: styleData.value
                    font.family: "Liberation Sans"
                    font.pointSize: 10
                }
            }
        }

        Rectangle {
            id: info
            anchors.left: parent.left
            anchors.right: parent.right
            anchors.top: parent.top
            height: 20
            color: "transparent"

            Text {
                anchors.fill: parent

                visible: true

                // Since --all-- is counted, count minus 1.
                text: roleString + " count: " + (list_model.count - 1)
                color: "#ffffff"
                verticalAlignment: Text.AlignVCenter
                horizontalAlignment: Text.AlignHCenter
                font.pointSize: 12
                font.family: "Helvetica"
                styleColor: "#000000"
                style: Text.Outline
            }
        }
    }
}
