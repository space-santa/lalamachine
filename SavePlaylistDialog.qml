import QtQuick 2.0
import QtQuick.Controls 1.2
import QtQuick.Dialogs 1.2

Dialog {
    id: savePlaylistDialog
    visible: false
    title: "Choose a name"
    property string playlistName: name_field.text
    standardButtons: StandardButton.Save | StandardButton.Cancel

    onAccepted: console.log("Saving the date " + name_field.text)
    onRejected: {
        visible = false
        name_field.text = ""
    }

    Column {
        anchors.top: parent.top
        anchors.left: parent.left
        anchors.right: parent.right

        spacing: 5

        Text {
            width: parent.width
            text: "Playlist name:"
        }

        TextField {
            id: name_field
            width: parent.width
        }
    }
}
