from PySide6.QtCore import QObject, Signal, Property
from PySide6.QtQml import QmlElement

QML_IMPORT_NAME = "Lala"
QML_IMPORT_MAJOR_VERSION = 1

@QmlElement
class LibSettings(QObject):
    isActiveChanged = Signal(bool)
    libraryTopShelveHeightChanged = Signal(int)

    def __init__(self, parent=None):
        super().__init__(parent)
        self._isActive = False
        self._libraryTopShelveHeight = 0

    @Property(bool, notify=isActiveChanged)
    def isActive(self):
        return self._isActive

    @isActive.setter
    def isActive(self, value):
        if self._isActive != value:
            self._isActive = value
            self.isActiveChanged.emit(value)

    @Property(bool, notify=libraryTopShelveHeightChanged)
    def libraryTopShelveHeight(self):
        return self._libraryTopShelveHeight

    @libraryTopShelveHeight.setter
    def libraryTopShelveHeight(self, value):
        if self._libraryTopShelveHeight != value:
            self._libraryTopShelveHeight = value
            self.libraryTopShelveHeightChanged.emit(value)
