import QtQuick 2.0

import "qrc:/functions.js" as Functions

Rectangle {
    id: container
    height: now_playing_container.height + btn_row.height + volume_control.height

    property var rawTitle
    property var rawAlbumArtist

    property alias duration: now_playing_container.duration
    property alias position: now_playing_container.position
    property alias hasAudio: now_playing_container.hasAudio

    property alias volume: volume_control.value

    property alias repeatOne: now_playing_container.repeatOne
    property alias repeatAll: now_playing_container.repeatAll
    property alias random: now_playing_container.random

    signal seek(var pos)

    function volumeUp() {
        volume_control.volumeUp()
    }
    function volumeDown() {
        volume_control.volumeDown()
    }

    Item {
        id: spacer
        anchors.top: parent.top
        anchors.bottom: now_playing_container.top
        anchors.right: parent.right
        anchors.left: parent.left
    }

    NowPlayingDisplay {
        id: now_playing_container
        anchors.bottom: btn_row.top
        anchors.right: parent.right
        width: parent.width

        title: Functions.getSafeValue(rawTitle)
        albumArtist: Functions.getSafeValue(rawAlbumArtist)

        onSeek: {
            container.seek(pos)
        }
    }

    PlayerControlButtons {
        id: btn_row
        anchors.left: parent.left
        anchors.bottom: parent.bottom
    }

    VolumeControl {
        id: volume_control
        anchors.bottom: parent.bottom
        anchors.left: btn_row.right
        anchors.right: parent.right
    }
}
