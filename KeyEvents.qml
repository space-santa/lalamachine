import QtQuick 2.0

Item {
    focus: true

    signal volumeUp
    signal volumeDown
    signal forward
    signal back
    signal pause
    signal deleteTrack
    signal startPlaying

    // This is to always get the focus back. Otherwise e.g. the playlist View
    // will keep the focus.
    onFocusChanged: focus = true

    Keys.onUpPressed: {
        volumeUp()
    }

    Keys.onDownPressed: {
        volumeDown()
    }

    Keys.onLeftPressed: {
        back()
    }

    Keys.onRightPressed: {
        forward()
    }

    Keys.onSpacePressed: {
        pause()
    }

    Keys.onDeletePressed: {
        deleteTrack()
    }

    Keys.onEnterPressed: {
        console.log("playing current row")
        startPlaying()
    }
}
