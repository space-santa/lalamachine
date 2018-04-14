#-------------------------------------------------
#
# Project created by QtCreator 2015-02-07T12:00:07
#
#-------------------------------------------------

QT       += testlib qml quick widgets sql concurrent

CONFIG += c++11

TARGET = tst_lalatesttest
CONFIG   += console
CONFIG   -= app_bundle

TEMPLATE = app


SOURCES += \
    ../../src/QueryBuilder.cpp \
    ../../src/QueryResult.cpp \
    ../../src/autoplaylist.cpp \
    ../../src/autoplaylistmanager.cpp \
    ../../src/autoplaylistobject.cpp \
    ../../src/config.cpp \
    ../../src/exceptions.cpp \
    ../../src/lalatypes.cpp \
    ../../src/playlistprovider.cpp \
    ../../src/MainDB.cpp \
    ../../src/metadataprovider.cpp \
    ../../src/model.cpp \
    ../../src/musiclibscanner.cpp \
    ../../src/tags.cpp \
    ../../src/timeconverter.cpp \
    QueryBuilder.test.cpp \
    lalatest.cpp \
    main.cpp \
    model.test.cpp \
    model.test.mock.cpp \
    musiclibscanner.test.cpp \
    musiclibscanner.test.mock.cpp \
    testautoplaylist.cpp \
    maindbtest.cpp

DEFINES += SRCDIR=\\\"$$PWD/\\\"

HEADERS += \
    ../../src/IDirWalker.h \
    ../../src/IMetaDataProvider.h \
    ../../src/IQueryResult.h \
    ../../src/IScannerDB.h \
    ../../src/QueryBuilder.h \
    ../../src/QueryResult.h \
    ../../src/autoplaylist.h \
    ../../src/autoplaylistmanager.h \
    ../../src/autoplaylistobject.h \
    ../../src/config.h \
    ../../src/exceptions.h \
    ../../src/lalatypes.h \
    ../../src/playlistprovider.h \
    ../../src/MainDB.h \
    ../../src/metadataprovider.h \
    ../../src/model.h \
    ../../src/musiclibscanner.h \
    ../../src/tags.h \
    ../../src/timeconverter.h \
    QueryBuilder.test.h \
    lalatest.h \
    model.test.h \
    model.test.mock.h \
    musiclibscanner.test.h \
    musiclibscanner.test.mock.h \
    testautoplaylist.h \
    maindbtest.h

unix!macx: {
    INCLUDEPATH += /usr/include/taglib
    LIBS += -ltag
}

macx: {
    INCLUDEPATH += /usr/local/include/taglib
    LIBS += -L/usr/local/lib/ -ltag
}

win32-g++: {
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/ape
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/asf
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/flac
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/it
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/mod
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/mp4
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/mpc
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/mpeg
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/mpeg/id3v2
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/ogg
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/riff
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/s3m
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/toolkit
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/trueaudio
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/wavpack
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/taglib/xm
    INCLUDEPATH += $$PWD/../../../taglib-1.11.1/build
    INCLUDEPATH += $$PWD/../../../build-taglib-1.11.1-Desktop_Qt_5_10_1_MinGW_32bit-Default
    CONFIG(release, debug|release): LIBS += $$PWD/../../../taglib-1.11.1/build/taglib/libtag.dll
}

DISTFILES += \
    musiclib.sq3
