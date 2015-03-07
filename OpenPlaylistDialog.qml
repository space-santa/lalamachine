import QtQuick 2.0
import QtQuick.Controls 1.2
import QtQuick.Dialogs 1.2

Dialog {
    width: 400
    height: 600

    visible: false
    standardButtons: StandardButton.Open | StandardButton.Cancel

    property string selection: namelist_model.get(namelist_view.currentRow).listname

    onSelectionChanged: console.log(selection)

    onAccepted: {
        selection = namelist_model.get(namelist_view.currentRow).listname
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
