# This Python file uses the following encoding: utf-8
import sys
from pathlib import Path

from PySide6.QtGui import QGuiApplication
from PySide6.QtQml import QQmlApplicationEngine
# from PySide6.QtQuickControls2 import QQuickStyle
from PySide6.QtQml import qmlRegisterType
from src.config import Config
from src.musiclib import MusicLib
from src.lalaplaylistprovider import LalaPlaylistProvider
from src.settings import Settings
from src.libsettings import LibSettings
from src.playlist_table_model import PlaylistTableModel

if __name__ == "__main__":
    app = QGuiApplication(sys.argv)

    qmlRegisterType(Config, "Lala", 1, 0, "Config")
    qmlRegisterType(MusicLib, "Lala", 1, 0, "MusicLib")
    qmlRegisterType(LalaPlaylistProvider, "Lala", 1, 0, "LalaPlaylistProvider")
    qmlRegisterType(Settings, "Lala", 1, 0, "Settings")
    qmlRegisterType(LibSettings, "Lala", 1, 0, "LibSettings")
    qmlRegisterType(PlaylistTableModel, "Lala", 1, 0, "PlaylistTableModel")

    # QQuickStyle.setStyle("Material")
    engine = QQmlApplicationEngine()
    qml_file = Path(__file__).resolve().parent / "assets" / "qml" / "main.qml"
    engine.load(qml_file)
    if not engine.rootObjects():
        sys.exit(-1)
    sys.exit(app.exec())
