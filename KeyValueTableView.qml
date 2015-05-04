import QtQuick 2.0
import QtQuick.Controls 1.2

Item {
    id: name
    property alias keyTitle: key_column.title
    property alias valueTitle: value_column.title

    // The list will contain the JsonArray.
    // Each JSON objct must have a "key" and a "value".
    property var list

    onListChanged: {
        updateList()
    }

    Component.onCompleted: updateList()

    function updateList() {
        model.clear()

        for (var i = 0; i < list.length; ++i) {
            model.append({
                             key: list[i].key,
                             value: list[i].value
                         })
        }
    }

    function getJson() {
        var retval = []
        for (var i = 0; i < model.count; ++i) {
            retval.push({
                            key: model[i].key,
                            value: model[i].value
                        })
        }
        return retval
    }

    function moveUp() {
        model.move(view.currentRow, view.currentRow - 1, 1)
    }

    function moveDown() {
        model.move(view.currentRow, view.currentRow + 1, 1)
    }

    function setSelectionEnabled(val) {
        model.setProperty(view.currentRow, "value", val)
    }

    ListModel {
        id: model
    }

    TableView {
        id: view

        TableViewColumn {
            id: key_column
            role: "key"
        }

        TableViewColumn {
            id: value_column
            role: "value"
        }

        rowDelegate: TableViewDelegate {
            target: view
            onRightClick: rcm.popup()
        }
    }

    Menu {
        id: rcm
        MenuItem {
            text: "enable"
            onTriggered: setSelectionEnabled(true)
        }
        MenuItem {
            text: "disable"
            onTriggered: setSelectionEnabled(false)
        }
    }
}
