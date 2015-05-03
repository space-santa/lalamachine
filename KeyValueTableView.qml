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
        model.clear()

        for (var i = 0; i < list.length; ++i) {
            model.append({
                             key: list[i].key,
                             value: list[i].value
                         })
        }
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
    }
}
