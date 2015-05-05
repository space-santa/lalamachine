import QtQuick 2.0
import QtQuick.Controls 1.2

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
        console.log("ZZZ current row", view.currentRow)
        if (view.currentRow > -1) {
            model.setProperty(view.currentRow, "value", val)
        }
    }

    ListModel {
        id: model
    }

    TableView {
        id: view
        anchors.fill: parent

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
