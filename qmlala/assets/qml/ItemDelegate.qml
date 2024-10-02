import QtQuick

TextBase {
    clip: true
    color: "white"
    text: styleData.value ? styleData.value : ""
    verticalAlignment: Qt.AlignVCenter
    horizontalAlignment: styleData.textAlignment
}
