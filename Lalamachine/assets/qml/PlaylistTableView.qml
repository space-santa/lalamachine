import QtQuick 2.0
import QtQuick.Controls 1.2

TableView {
    id: playlist_view
    selectionMode: SelectionMode.ExtendedSelection
    backgroundVisible: true

    style: TableStyle {
    }

    property var playlistColumns: playlist_container.playlistColumns

    // These values are required for drag and move. For a explanation,
    // see TableViewDelegate.qml.
    property int mouseY
    property int containerHeight
    property int initialRow
    property int currentRow

    Component.onCompleted: setColumns()
    onPlaylistColumnsChanged: setColumns()

    // Since we are handling left clicks in the delegate we are effectively
    // disabling ctrl-click/shift-click here.
    // Hence a naive attempt to implement that functionality manually.
    // When clicked into the table, we force active focus to get key events.
    // Hitting ctrl/shift with focus sets the appropriat flag to true.
    // Releasing the key sets its flag to false.
    // Hitting e.g. shift while still holding ctrl will set ctrl to false.
    // That is the highlander principle. There can only be one.
    property bool ctrlPressed: false
    property bool shiftPressed: false

    // The lastSelection property is required for shift-click action.
    // We need to now the range to select.
    property int lastSelection: -1

    // We need a flag to determine if the mouse button is currently pressed.
    // If it is not pressed, we can't have a drag action.
    property bool leftPressed: false

    onActiveFocusChanged: {
        if (!focus) {
            ctrlPressed = false
            shiftPressed = false
        }
    }

    Keys.onPressed: {
        if (event.key === Qt.Key_Control) {
            shiftPressed = false
            ctrlPressed = true
        }
        if (event.key === Qt.Key_Shift) {
            ctrlPressed = false
            shiftPressed = true
        }
    }

    Keys.onReleased: {
        if (event.key === Qt.Key_Control) {
            ctrlPressed = false
        }
        if (event.key === Qt.Key_Shift) {
            shiftPressed = false
        }
    }

    onMouseYChanged: {
        // Since the library can't play tracks in order,
        // moving them around doesn't make sense.
        if (isLibrary) {
            return
        }
        // If ctrl or shift is pressed it's not dragging, it's selecting.
        if (ctrlPressed || shiftPressed) {
            return
        }

        // If the mouse button is not pressed, no drag.
        if (!leftPressed) {
            return
        }

        var newRow = initialRow + mouseY / containerHeight
        // console.log("Current Row", currentRow, "New Row", newRow, "mouseY", mouseY, "cheight", containerHeight)

        if (newRow < 0) {
            return
        }

        if (Math.abs(newRow - initialRow) < 0.1) {
            return
        }

        selection.clear()
        model.move(currentRow, newRow, 1)
        currentRow = newRow
    }

    onClicked: {
        forceActiveFocus()
        currentRow = row
        if (!ctrlPressed) {
            selection.clear()
        }

        if (shiftPressed) {
            selection.select(lastSelection, row)
        } else {
            selection.select(row)
            lastSelection = row
        }
    }

    function setColumns() {
        for (var i = columnCount - 1; i >= 0; --i) {
            removeColumn(i)
        }

        let columnsJson = JSON.parse(playlistColumns);

        for (let i = 0; i < columnsJson.length; ++i) {
            if (columnsJson[i].value) {
                let columnString = buildColumnString(columnsJson[i].key);
                var o = Qt.createQmlObject(columnString, playlist_view, "DynO");
                playlist_view.addColumn(o)
            }
        }
    }

    function buildColumnString(tag) {
        var width = 150
        var title = tag

        if (tag === "track") {
            width = 70
        }
        if (tag === "title") {
            width = 250
        }
        if (tag === "comment") {
            width = 100
        }
        if (tag === "length") {
            width = 80
            tag = "lengthString"
        }
        if (tag === "genre") {
            width = 150
            tag = "genreString"
        }
        if (tag === "artist") {
            width = 150
            tag = "artistString"
        }
        if (tag === "album") {
            width = 150
        }
        if (tag === "year") {
            width = 70
        }
        if (tag === "discNumber") {
            width = 50
            title = "Disc"
        }

        return columnString(tag, title, width)
    }

    function columnString(role, title, width) {
        var retval = "import QtQuick 2.0;\n"
        retval += "import QtQuick.Controls 1.2;\n"
        retval += "TableViewColumn {\n"
        retval += "role: \"" + role + "\";\n"
        retval += "title: \"" + title + "\";\n"
        retval += "width: " + width + "\n"
        retval += "}"

        return retval
    }

    onModelChanged: {
        if (model.rowCount() > 0) {
            playlist_view.resizeColumnsToContents()
            updateNowPlayingRow()
        }
    }

    function getColumnRole(col) {
        return getColumn(col).role
    }

    sortIndicatorVisible: true
    onSortIndicatorColumnChanged: {
        model.sortRole(getColumnRole(sortIndicatorColumn), sortIndicatorOrder)
    }
    onSortIndicatorOrderChanged: {
        model.sortRole(getColumnRole(sortIndicatorColumn), sortIndicatorOrder)
    }

    rowDelegate: TableViewDelegate {
        target: playlist_view
        onRightClick: showRightClickMenu()

        onDoubleClicked: {
            stop()
            playRow(row)
        }

        onPressed: target.leftPressed = true
        onReleased: target.leftPressed = false

        onMouseYChanged: {
            //console.log("MOUSE Y CHANGED", y, height, row)
            target.mouseY = y
            target.containerHeight = height
            target.initialRow = row
            target.currentRow = row
        }
    }

    itemDelegate: ItemDelegate {
    }

    headerDelegate: TableHeaderDelegate {
    }
}
