#-------------------------------------------------
#
# Project created by QtCreator 2015-02-07T12:00:07
#
#-------------------------------------------------

QT       += testlib qml quick widgets sql concurrent

CONFIG += c++11

INCLUDEPATH += /usr/include/taglib

LIBS += -ltag

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
    ../../src/m3uinout.cpp \
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
    testautoplaylist.cpp

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
    ../../src/m3uinout.h \
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
    testautoplaylist.h

unix!macx: {
    INCLUDEPATH += /usr/include/taglib
    LIBS += -ltag
}

macx: {
    INCLUDEPATH += /usr/local/include/taglib
    LIBS += -L/usr/local/lib/ -ltag
}
