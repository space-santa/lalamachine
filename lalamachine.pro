TEMPLATE = app

QT += qml quick widgets sql concurrent

CONFIG += C++11

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
    sysinfo.cpp \
    musiclibscanner.cpp \
    autoplaylistobject.cpp \
    lalatypes.cpp \
    autoplaylistmanager.cpp \
    fileexporter.cpp \
    tags.cpp \
    autoplaylist.cpp \
    playlistmodel.cpp \
    track.cpp \
    engineloader.cpp \
    lalatray.cpp

RESOURCES += qml.qrc

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
    sysinfo.h \
    musiclibscanner.h \
    autoplaylistobject.h \
    lalatypes.h \
    autoplaylistmanager.h \
    fileexporter.h \
    engineloader.h \
    tags.h \
    autoplaylist.h \
    playlistmodel.h \
    track.h \
    lalatray.h

OTHER_FILES += \
    README.md \
    images/license-icons.txt \
    lalamachine.desktop \
    lalamachine.spec

DISTFILES += \
    LICENSE
