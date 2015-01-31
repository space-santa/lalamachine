import QtQuick 2.0

Row {
    id: btn_row

    width: childrenRect.width
    height: childrenRect.height

    signal playPrevious
    signal play
    signal pause
    signal open
    signal playNext

    ImageButton {
        id: back_btn
        source: "qrc:/images/images/back.png"
        onClicked: playPrevious()
    }

    ImageButton {
        id: play_btn
        source: "qrc:/images/images/play.png"
        onClicked: play()
    }

    ImageButton {
        id: pause_btn
        source: "qrc:/images/images/pause.png"
        onClicked: pause()
    }

    ImageButton {
        id: open_btn
        source: "qrc:/images/images/eject.png"
        onClicked: open()
    }

    ImageButton {
        id: next_btn
        source: "qrc:/images/images/forward.png"
        onClicked: playNext()
    }
}
