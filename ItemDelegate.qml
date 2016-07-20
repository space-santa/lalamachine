import QtQuick 2.0

Text {
    font.family: "Helvetica"
    font.pointSize: 10
    clip: true
    color: styleData.selected ? "lightgrey" : "black"
    text: styleData.value ? styleData.value : ""
    verticalAlignment: Qt.AlignVCenter
    horizontalAlignment: styleData.textAlignment
}
