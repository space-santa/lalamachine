TEMPLATE = app

QT += qml quick widgets sql concurrent

CONFIG += C++11

#INCLUDEPATH += /usr/include/taglib
#INCLUDEPATH += /usr/include/vlc

#LIBS += -ltag -lvlc

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
    src/lalatray.cpp \
    src/theplayer.cpp \
    src/theplayeventhandler.cpp

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
    src/lalatray.h \
    src/theplayer.h \
    src/theplayeventhandler.h

OTHER_FILES += \
    README.md \
    images/license-icons.txt \
    lalamachine.desktop \
    lalamachine.spec

DISTFILES += \
    LICENSE \
    changelog

win32:CONFIG(release, debug|release): LIBS += -L$$PWD/../../../../Code/taglib-1.11/build/taglib/ -ltag
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/ape
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/asf
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/flac
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/it
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/mod
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/mp4
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/mpc
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/mpeg
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/ogg
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/riff
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/s3m
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/toolkit
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/trueaudio
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/wavpack
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/taglib/xm
INCLUDEPATH += $$PWD/../../../../Code/taglib-1.11/build

win32:CONFIG(release, debug|release): LIBS += -L$$PWD/../../../../Code/libvlc/lib/ -lvlc
INCLUDEPATH += $$PWD/../../../../Code/libvlc/include
INCLUDEPATH += $$PWD/../../../../Code/libvlc/include/vlc
INCLUDEPATH += $$PWD/../../../../Code/libvlc/include/vlc/plugins
