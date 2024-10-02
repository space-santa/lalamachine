from PySide6.QtCore import QObject, Signal, Property, Slot
from PySide6.QtQml import QmlElement
import json
QML_IMPORT_NAME = "Lala"
QML_IMPORT_MAJOR_VERSION = 1

@QmlElement
class LalaPlaylistProvider(QObject):
    playlistNamesChanged = Signal()
    playlistColumnsChanged = Signal(str)

    def __init__(self, parent=None):
        super().__init__(parent)
        self._volume = 0
        self._libPath= ""
        self._lastPlaylist = ""
        self._playlistColumns = [
        { "key": "track", "value": True },
        { "key": "discNumber", "value": True },
        { "key": "title", "value": True },
        { "key": "comment", "value": True },
        { "key": "length", "value": True },
        { "key": "genre", "value": True },
        { "key": "album", "value": True },
        { "key": "artist", "value": True },
        { "key": "year", "value": True },
    ]

    @Slot(str, str)
    def writePlaylist(self, name, json):
        pass

    @Property(str)
    def allPlaylistNames(self):
        return json.dumps([])

    @Property(str)
    def playlistColumns(self):
        return json.dumps(self._playlistColumns)

    @playlistColumns.setter
    def playlistColumns(self, value):
        json_value = json.loads(value)
        if json_value != self._playlistColumns:
            self._playlistColumns = json_value
            self.playlistColumnsChanged.emit(json_value)

