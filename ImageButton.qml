import QtQuick 2.0
import QtQuick.Controls 1.2
import QtQuick.Controls.Styles 1.2

Button {
    property string source
    Image {
        anchors.centerIn: parent
        width: parent.width - 10
        height: parent.height - 10
        source: parent.source
    }
    style: ButtonStyle {
        background: Rectangle {
            anchors.fill: parent
            color: control.pressed ? "lightgrey" : "transparent"
        }
    }
}
