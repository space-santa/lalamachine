TEMPLATE = app

QT += qml quick widgets sql concurrent multimedia quickwidgets

CONFIG += C++11 # console
QMAKE_CXXFLAGS += -Werror

SOURCES += \
    src/DirWalker.cpp \
    src/MainDB.cpp \
    src/QueryBuilder.cpp \
    src/QueryResult.cpp \
    src/ScannerDB.cpp \
    src/autoplaylist.cpp \
    src/autoplaylistmanager.cpp \
    src/autoplaylistobject.cpp \
    src/config.cpp \
    src/exceptions.cpp \
    src/fileexporter.cpp \
    src/lalatypes.cpp \
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
    src/track.cpp \
    src/playlistprovider.cpp

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
    src/QueryResult.h \
    src/ScannerDB.h \
    src/autoplaylist.h \
    src/autoplaylistmanager.h \
    src/autoplaylistobject.h \
    src/config.h \
    src/exceptions.h \
    src/fileexporter.h \
    src/lalatypes.h \
    src/metadataprovider.h \
    src/model.h \
    src/musiclib.h \
    src/musiclibscanner.h \
    src/playlistmodel.h \
    src/sysinfo.h \
    src/tags.h \
    src/theplayer.h \
    src/timeconverter.h \
    src/track.h \
    src/playlistprovider.h

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
}

win32-g++: {
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/ape
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/asf
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/flac
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/it
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/mod
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/mp4
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/mpc
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/mpeg
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/mpeg/id3v2
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/ogg
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/riff
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/s3m
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/toolkit
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/trueaudio
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/wavpack
    INCLUDEPATH += $$PWD/../taglib-1.11.1/taglib/xm
    INCLUDEPATH += $$PWD/../taglib-1.11.1/build
    INCLUDEPATH += $$PWD/../build-taglib-1.11.1-Desktop_Qt_5_10_1_MinGW_32bit-Default
    CONFIG(release, debug|release): LIBS += $$PWD/../taglib-1.11.1/build/taglib/libtag.dll
    #CONFIG(release, debug|release): PRE_TARGETDEPS += $$PWD/../taglib-1.11.1/taglib/build/taglib/libtag.dll
}
