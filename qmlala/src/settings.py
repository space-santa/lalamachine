from PySide6.QtCore import Property, QObject, Signal
from PySide6.QtQml import QmlElement

QML_IMPORT_NAME = "Lala"
QML_IMPORT_MAJOR_VERSION = 1


@QmlElement
class Settings(QObject):
    xChanged = Signal(int)
    yChanged = Signal(int)
    hChanged = Signal(int)
    wChanged = Signal(int)
    isActiveChanged = Signal(bool)

    def __init__(self, parent=None):
        super().__init__(parent)
        self._x = 0
        self._y = 0
        self._h = 400
        self._w = 800
        self._isActive = False

    @Property(int)
    def x(self):
        return self._x

    @x.setter
    def x(self, value):
        if self._x != value:
            self._x = value
            self.xChanged.emit(value)

    @Property(int)
    def y(self):
        return self._y

    @y.setter
    def y(self, value):
        if self._y != value:
            self._y = value
            self.yChanged.emit(value)

    @Property(int)
    def h(self):
        return self._h

    @h.setter
    def h(self, value):
        if self._h != value:
            self._h = value
            self.hChanged.emit(value)

    @Property(int)
    def w(self):
        return self._w

    @w.setter
    def w(self, value):
        if self._w != value:
            self._w = value
            self.wChanged.emit(value)

    @Property(bool)
    def isActive(self):
        return self._isActive

    @isActive.setter
    def isActive(self, value):
        if self._isActive != value:
            self._isActive = value
            self.isActiveChanged.emit(value)
