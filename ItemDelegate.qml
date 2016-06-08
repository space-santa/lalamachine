import QtQuick 2.0

Text {
    anchors.fill: parent
    anchors.leftMargin: 10
    anchors.rightMargin: 10
    font.family: "Helvetica"
    font.pointSize: 10
    clip: true
    color: styleData.selected ? "lightgrey" : "black"
    text: styleData.value ? styleData.value : ""
    verticalAlignment: Qt.AlignVCenter
    horizontalAlignment: styleData.textAlignment
}
