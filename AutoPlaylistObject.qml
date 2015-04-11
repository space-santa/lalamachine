import QtQuick 2.0
import QtQuick.Layouts 1.1
import QtQuick.Controls 1.3

GridLayout {
    width: 100
    height: 62

    flow: GridLayout.TopToBottom
    rows: 2

    Text {
        text: "How to apply?"
    }
    ComboBox {
        model: ["and", "or"]
    }

    Text {
        text: "Where?"
    }
    ComboBox {
        model: ["Genre", "Artist", "Album", "Comment", "Title"]
    }

    Text {
        text: "How?"
    }
    ComboBox {
        model: ["contains", "contains not", "is", "is not"]
    }

    Text {
        text: "Value?"
    }
    TextField {
        id: value_field
    }
}

