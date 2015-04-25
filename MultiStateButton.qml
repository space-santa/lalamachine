import QtQuick 2.0


ImageButton {
    id: repeat_btn
    width: 50
    source: imageSource()
    checkable: true
    checked: none ? false : true

    property bool none: true
    property bool repeatAll: false
    property bool repeatOne: false
    property bool random: false

    signal selectionChanged

    onSelectionChanged: checkSelection()

    onNoneChanged: {
        if (none) {
            console.log("none")
            selectionChanged()
        }
    }
    onRepeatAllChanged: {
        if (repeatAll) {
            console.log("repeatAll")
            selectionChanged()
        }
    }
    onRepeatOneChanged: {
        if (repeatOne) {
            console.log("repeatOne")
            selectionChanged()
        }
    }
    onRandomChanged: {
        if (random) {
            console.log("random")
            selectionChanged()
        }
    }

    function checkSelection() {
        if (none) {
            repeatAll = false
            repeatOne = false
            random = false
        }
        if (repeatAll) {
            none = false
            repeatOne = false
            random = false
        }
        if (repeatOne) {
            none = false
            repeatAll = false
            random = false
        }
        if (random) {
            repeatAll = false
            repeatOne = false
            none = false
        }
    }

    function imageSource() {
        if (repeatOne) {
            return "qrc:/images/images/repeat1.png"
        }
        if (repeatAll) {
            return "qrc:/images/images/repeatAll.png"
        }
        if (random) {
            return "qrc:/images/images/random.png"
        }
        return "qrc:/images/images/repeat_random.png"
    }

    MouseArea {
        anchors.fill: parent

        onClicked: {
            if (none) {
                none = false
                repeatOne = true
            } else if (repeatOne) {
                repeatOne = false
                repeatAll = true
            } else if (repeatAll) {
                repeatAll = false
                random = true
            } else {
                random = false
                none = true
            }
        }
    }
}
