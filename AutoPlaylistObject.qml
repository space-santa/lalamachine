import QtQuick 2.0
import QtQuick.Layouts 1.1
import QtQuick.Controls 1.3

Item {
    height: 75

    property alias andorText: andorbox.currentText
    property alias whereText: wherebox.currentText
    property alias howText: howbox.currentText
    property alias valueText: value_field.text

    function init() {
        andorbox.currentIndex = 0
        wherebox.currentIndex = 0
        howbox.currentIndex = 0
        value_field.text = ""
    }

    function set(andor, where, how, value) {
        andorbox.currentIndex = andorbox.find(translate(andor))
        wherebox.currentIndex = wherebox.find(translate(where))
        howbox.currentIndex = howbox.find(translate(how))
        value_field.text = value
    }

    Item {
        id: grid
        anchors.top: parent.top
        anchors.left: parent.left
        anchors.bottom: parent.bottom

        width: 385
        height: 75

        Text {
            id: andor_label
            anchors.top: parent.top
            anchors.left: parent.left
            anchors.margins: 5
            width: 120
            height: 30
            text: "How to apply?"
        }
        ComboBox {
            id: andorbox
            anchors.top: andor_label.bottom
            anchors.left: parent.left
            anchors.margins: 5
            width: 120
            height: 30
            model: ["and", "or"]
        }

        Text {
            id: where_label
            anchors.top: parent.top
            anchors.left: andor_label.right
            anchors.margins: 5
            width: 120
            height: 30
            text: "Where?"
        }
        ComboBox {
            id: wherebox
            anchors.top: where_label.bottom
            anchors.left: andorbox.right
            anchors.margins: 5
            width: 120
            height: 30
            model: ["Genre", "Artist", "Album", "Comment", "Title"]
        }

        Text {
            id: how_label
            anchors.top: parent.top
            anchors.left: where_label.right
            anchors.margins: 5
            width: 120
            height: 30
            text: "How?"
        }
        ComboBox {
            id: howbox
            anchors.top: how_label.bottom
            anchors.left: wherebox.right
            anchors.margins: 5
            width: 120
            height: 30
            model: ["contains", "contains not", "is", "is not"]
        }
    }

    Text {
        id: value_label
        anchors.left: grid.right
        anchors.top: parent.top
        anchors.right: parent.right
        anchors.margins: 5

        height: 30
        text: "Value?"
    }
    TextField {
        id: value_field
        anchors.left: grid.right
        anchors.top: value_label.bottom
        anchors.right: parent.right
        anchors.margins: 5

        height: 30
    }
}
