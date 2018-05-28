import QtQuick 2.0

TextBase {
    clip: true
    color: "white"
    text: styleData.value ? styleData.value : ""
    verticalAlignment: Qt.AlignVCenter
    horizontalAlignment: styleData.textAlignment
}
