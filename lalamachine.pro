TEMPLATE = app

QT += qml quick widgets sql concurrent

CONFIG += C++11

INCLUDEPATH += /usr/include/taglib

LIBS += -ltag

SOURCES += src/main.cpp \
    src/metadataprovider.cpp \
    src/m3uinout.cpp \
    src/config.cpp \
    src/timeconverter.cpp \
    src/musiclib.cpp \
    src/cdwriter.cpp \
    src/systeminteractor.cpp \
    src/sysinfo.cpp \
    src/musiclibscanner.cpp \
    src/autoplaylistobject.cpp \
    src/lalatypes.cpp \
    src/autoplaylistmanager.cpp \
    src/fileexporter.cpp \
    src/tags.cpp \
    src/autoplaylist.cpp \
    src/playlistmodel.cpp \
    src/track.cpp \
    src/engineloader.cpp \
    src/lalatray.cpp

RESOURCES += qml.qrc

# Default rules for deployment.
include(deployment.pri)

HEADERS += \
    src/metadataprovider.h \
    src/m3uinout.h \
    src/config.h \
    src/timeconverter.h \
    src/musiclib.h \
    src/cdwriter.h \
    src/systeminteractor.h \
    src/sysinfo.h \
    src/musiclibscanner.h \
    src/autoplaylistobject.h \
    src/lalatypes.h \
    src/autoplaylistmanager.h \
    src/fileexporter.h \
    src/engineloader.h \
    src/tags.h \
    src/autoplaylist.h \
    src/playlistmodel.h \
    src/track.h \
    src/lalatray.h

OTHER_FILES += \
    README.md \
    images/license-icons.txt \
    lalamachine.desktop \
    lalamachine.spec

DISTFILES += \
    LICENSE
