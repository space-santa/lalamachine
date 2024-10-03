import json

from PySide6.QtCore import Property, QObject, Signal
from PySide6.QtQml import QmlElement

QML_IMPORT_NAME = "Lala"
QML_IMPORT_MAJOR_VERSION = 1


@QmlElement
class Config(QObject):
    volumeChanged = Signal(float)
    playlistColumnsChanged = Signal()

    def __init__(self, parent=None):
        super().__init__(parent)
        self._volume = 0.5
        self._libPath = ""
        self._lastPlaylist = ""
        self._playlistColumns = [
            {"key": "track", "value": True},
            {"key": "discNumber", "value": True},
            {"key": "title", "value": True},
            {"key": "comment", "value": True},
            {"key": "length", "value": True},
            {"key": "genre", "value": True},
            {"key": "album", "value": True},
            {"key": "artist", "value": True},
            {"key": "year", "value": True},
        ]

    @Property(float, notify=volumeChanged)
    def volume(self):
        return self._volume

    @volume.setter
    def volume(self, value):
        if self._volume != value:
            self._volume = value
            self.volumeChanged.emit(self._volume)

    @Property(str, notify=playlistColumnsChanged)
    def playlistColumns(self):
        return json.dumps(self._playlistColumns)

    @playlistColumns.setter
    def playlistColumns(self, value):
        if self._playlistColumns != value:
            self._playlistColumns = value
            self.playlistColumnsChanged.emit()

    @Property(str)
    def libPath(self):
        return self._libPath

    @Property(str)
    def lastPlaylist(self):
        return self._lastPlaylist
