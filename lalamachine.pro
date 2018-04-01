TEMPLATE = app

QT += qml quick widgets sql concurrent multimedia quickwidgets

CONFIG += C++11 # console
QMAKE_CXXFLAGS += -Werror

SOURCES += \
    src/DirWalker.cpp \
    src/MainDB.cpp \
    src/QueryBuilder.cpp \
    src/ScannerDB.cpp \
    src/autoplaylist.cpp \
    src/autoplaylistmanager.cpp \
    src/autoplaylistobject.cpp \
    src/config.cpp \
    src/exceptions.cpp \
    src/fileexporter.cpp \
    src/lalatypes.cpp \
    src/m3uinout.cpp \
    src/main.cpp \
    src/metadataprovider.cpp \
    src/model.cpp \
    src/musiclib.cpp \
    src/musiclibscanner.cpp \
    src/playlistmodel.cpp \
    src/sysinfo.cpp \
    src/tags.cpp \
    src/theplayer.cpp \
    src/timeconverter.cpp \
    src/track.cpp

RESOURCES += qml.qrc

# Default rules for deployment.
include(deployment.pri)

HEADERS += \
    src/DirWalker.h \
    src/IDirWalker.h \
    src/IMainDB.h \
    src/IScannerDB.h \
    src/MainDB.h \
    src/QueryBuilder.h \
    src/ScannerDB.h \
    src/autoplaylist.h \
    src/autoplaylistmanager.h \
    src/autoplaylistobject.h \
    src/config.h \
    src/exceptions.h \
    src/fileexporter.h \
    src/lalatypes.h \
    src/m3uinout.h \
    src/metadataprovider.h \
    src/model.h \
    src/musiclib.h \
    src/musiclibscanner.h \
    src/playlistmodel.h \
    src/sysinfo.h \
    src/tags.h \
    src/theplayer.h \
    src/timeconverter.h \
    src/track.h

OTHER_FILES += \
    README.md \
    images/license-icons.txt \
    lalamachine.desktop \
    lalamachine.spec

DISTFILES += \
    LICENSE \
    changelog

ICON = ./images/lala-icon.png.icns

macx: {
    INCLUDEPATH += /usr/local/include/taglib
    LIBS += -L/usr/local/lib/ -ltag
}

linux-g++: {
    INCLUDEPATH += /usr/include/taglib
    LIBS += -ltag
    INCLUDEPATH += /usr/include/santa
    LIBS += /usr/lib/libsingleinstance.so
}

win32-g++: {
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
    CONFIG(release, debug|release): LIBS += -L$$PWD/../../../../Code/taglib-1.11-mingw/ -ltag
    CONFIG(release, debug|release): PRE_TARGETDEPS += $$PWD/../../../../Code/taglib-1.11-mingw/libtag.dll
}
