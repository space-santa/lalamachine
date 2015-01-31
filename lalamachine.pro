TEMPLATE = app

QT += qml quick widgets

INCLUDEPATH += /usr/include/taglib

LIBS += -ltag

SOURCES += main.cpp \
    metadataprovider.cpp

RESOURCES += qml.qrc

# Additional import path used to resolve QML modules in Qt Creator's code model
QML_IMPORT_PATH =

# Default rules for deployment.
include(deployment.pri)

HEADERS += \
    metadataprovider.h

OTHER_FILES += \
    README.md
