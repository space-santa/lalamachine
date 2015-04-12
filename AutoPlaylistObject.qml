import QtQuick 2.0
import QtQuick.Layouts 1.1
import QtQuick.Controls 1.3

GridLayout {
    width: 100
    height: 62

    flow: GridLayout.TopToBottom
    rows: 2

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
        andorbox.currentText = andor
        wherebox.currentText = where
        howbox.currentText = how
        value_field.text = value
    }

    Text {
        text: "How to apply?"
    }
    ComboBox {
        id: andorbox
        model: ["and", "or"]
    }

    Text {
        text: "Where?"
    }
    ComboBox {
        id: wherebox
        model: ["Genre", "Artist", "Album", "Comment", "Title"]
    }

    Text {
        text: "How?"
    }
    ComboBox {
        id: howbox
        model: ["contains", "contains not", "is", "is not"]
    }

    Text {
        text: "Value?"
    }
    TextField {
        id: value_field
    }
}
