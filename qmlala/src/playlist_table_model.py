from PySide6.QtCore import QAbstractTableModel, Qt, Slot
from PySide6.QtQml import QmlElement

QML_IMPORT_NAME = "Lala"
QML_IMPORT_MAJOR_VERSION = 1


@QmlElement
class PlaylistTableModel(QAbstractTableModel):
    def __init__(self, parent=None):
        super().__init__(parent)
        self._data = {}

    def rowCount(self, parent=None):
        return len(self._data)

    def columnCount(self, parent=None):
        return len(self._data[0]) if self._data else 0

    def data(self, index, role=Qt.DisplayRole):
        if role == Qt.DisplayRole:
            return self._data[index.row()][index.column()]
        return None

    def headerData(self, section, orientation, role=Qt.DisplayRole):
        if role == Qt.DisplayRole:
            if orientation == Qt.Horizontal:
                return f"Column {section + 1}"
            else:
                return f"Row {section + 1}"
        return None

    @Slot()
    def clear(self):
        pass

    @Slot(str, str, int)
    def appendColumn(self, tag: str, title: str, width: int):
        print(tag, title, width)
