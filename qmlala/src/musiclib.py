import json

from PySide6.QtCore import Property, QObject, Signal
from PySide6.QtQml import QmlElement

QML_IMPORT_NAME = "Lala"
QML_IMPORT_MAJOR_VERSION = 1


@QmlElement
class MusicLib(QObject):
    scanDone = Signal()
    genreListChanged = Signal()
    artistListChanged = Signal()
    albumListChanged = Signal()
    displayLibChanged = Signal()

    def __init__(self, parent=None):
        super().__init__(parent)
        self._scanning = False
        self._searchString = ""
        self._genreFilter = ""
        self._artistFilter = ""
        self._albumFilter = ""

    @Property(bool, notify=scanDone)
    def scanning(self):
        return self._scanning

    @scanning.setter
    def scanning(self, value):
        self._scanning = value

    @Property(str)
    def searchString(self):
        return self._searchString

    @searchString.setter
    def searchString(self, value):
        self._searchString = value

    @Property(str)
    def genreFilter(self):
        return self._genreFilter

    @genreFilter.setter
    def genreFilter(self, value):
        self._genreFilter = value

    @Property(str)
    def artistFilter(self):
        return self._artistFilter

    @artistFilter.setter
    def artistFilter(self, value):
        self._artistFilter = value

    @Property(str)
    def albumFilter(self):
        return self._albumFilter

    @albumFilter.setter
    def albumFilter(self, value):
        self._albumFilter = value

    @Property(str, notify=genreListChanged)
    def genreList(self):
        return json.dumps([])

    @Property(str, notify=artistListChanged)
    def artistList(self):
        return json.dumps([])

    @Property(str, notify=albumListChanged)
    def albumList(self):
        return json.dumps([])

    @Property(str, notify=displayLibChanged)
    def displayLib(self):
        return json.dumps([])
