import QtQuick 2.0

Column {
    height: childrenRect.height
    width: childrenRect.width
    signal moveTop
    signal moveUp
    signal moveDown
    signal moveBottom
    signal clearList

    ImageButton {
        id: top_btn
        width: 50
        source: "qrc:/images/images/go-top.png"
        onClicked: moveTop()
    }
    ImageButton {
        id: up_btn
        width: 50
        source: "qrc:/images/images/go-up.png"
        onClicked: moveUp()
    }
    ImageButton {
        id: down_btn
        width: 50
        source: "qrc:/images/images/go-down.png"
        onClicked: moveDown()
    }
    ImageButton {
        id: bottom_btn
        width: 50
        source: "qrc:/images/images/go-bottom.png"
        onClicked: moveBottom()
    }
    ImageButton {
        id: clear_btn
        width: 50
        source: "qrc:/images/images/edit-clear.png"
        onClicked: clearList()
    }
}
