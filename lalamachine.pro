TEMPLATE = app

CONFIG += c++11

QT += qml quick widgets sql concurrent

INCLUDEPATH += /usr/include/taglib

LIBS += -ltag

SOURCES += main.cpp \
    metadataprovider.cpp \
    m3uinout.cpp \
    config.cpp \
    timeconverter.cpp \
    musiclib.cpp \
    cdwriter.cpp \
    systeminteractor.cpp \
    playlistsorter.cpp \
    sysinfo.cpp \
    musiclibscanner.cpp \
    autoplaylistobject.cpp \
    lalatypes.cpp \
    autoplaylistmanager.cpp \
    fileexporter.cpp \
    tags.cpp \
    autoplaylist.cpp

RESOURCES += qml.qrc

# Additional import path used to resolve QML modules in Qt Creator's code model
QML_IMPORT_PATH =

# Default rules for deployment.
include(deployment.pri)

HEADERS += \
    metadataprovider.h \
    m3uinout.h \
    config.h \
    timeconverter.h \
    musiclib.h \
    cdwriter.h \
    systeminteractor.h \
    playlistsorter.h \
    sysinfo.h \
    musiclibscanner.h \
    autoplaylistobject.h \
    lalatypes.h \
    autoplaylistmanager.h \
    fileexporter.h \
    engineloader.h \
    tags.h \
    autoplaylist.h

OTHER_FILES += \
    README.md \
    images/license-icons.txt \
    lalamachine.desktop \
    lalamachine.spec

DISTFILES += \
    LICENSE
