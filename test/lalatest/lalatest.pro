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
    ../../src/config.cpp \
    ../../src/exceptions.cpp \
    ../../src/lalatypes.cpp \
    ../../src/playlistprovider.cpp \
    ../../src/playlistmodel.cpp \
    ../../src/MainDB.cpp \
    ../../src/metadataprovider.cpp \
    ../../src/model.cpp \
    ../../src/musiclibscanner.cpp \
    ../../src/tags.cpp \
    ../../src/track.cpp \
    ../../src/timeconverter.cpp \
    QueryBuilder.test.cpp \
    lalatest.cpp \
    main.cpp \
    model.test.cpp \
    model.test.mock.cpp \
    maindbtest.cpp \
    playlistmodeltest.cpp

DEFINES += SRCDIR=\\\"$$PWD/\\\"

HEADERS += \
    ../../src/IDirWalker.h \
    ../../src/IMetaDataProvider.h \
    ../../src/IQueryResult.h \
    ../../src/IScannerDB.h \
    ../../src/QueryBuilder.h \
    ../../src/QueryResult.h \
    ../../src/config.h \
    ../../src/exceptions.h \
    ../../src/lalatypes.h \
    ../../src/playlistprovider.h \
    ../../src/MainDB.h \
    ../../src/metadataprovider.h \
    ../../src/playlistmodel.h \
    ../../src/model.h \
    ../../src/musiclibscanner.h \
    ../../src/tags.h \
    ../../src/track.h \
    ../../src/timeconverter.h \
    QueryBuilder.test.h \
    lalatest.h \
    model.test.h \
    model.test.mock.h \
    maindbtest.h \
    playlistmodeltest.h

macx: {
    QMAKE_CXXFLAGS += -Werror
    INCLUDEPATH += /usr/local/include/taglib
    LIBS += -L/usr/local/lib/ -ltag
}

linux-g++: {
    QMAKE_CXXFLAGS += -Werror
    INCLUDEPATH += /usr/include/taglib
    LIBS += -ltag
}

win32-g++ {
    QMAKE_CXXFLAGS += -Werror
    LIBS += -LD:/lib/taglib/lib/ -llibtag.dll
    INCLUDEPATH += D:/lib/taglib/include/taglib
    DEPENDPATH += D:/lib/taglib/include/taglib
}

win32-msvc* {
    LIBS += -L$$PWD/../../../Libraries/taglib/lib/ -ltag
    INCLUDEPATH += $$PWD/../../../Libraries/taglib/include/taglib
    DEPENDPATH += $$PWD/../../../Libraries/taglib/include
}

DISTFILES += \
    musiclib.sq3
