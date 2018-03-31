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
    ../../src/config.cpp \
    ../../src/m3uinout.cpp \
    ../../src/metadataprovider.cpp \
    ../../src/timeconverter.cpp \
    ../../src/autoplaylistobject.cpp \
    ../../src/autoplaylistmanager.cpp \
    ../../src/autoplaylist.cpp \
    ../../src/lalatypes.cpp \
    ../../src/model.cpp \
    ../../src/musiclibscanner.cpp \
    main.cpp \
    lalatest.cpp \
    test_model.cpp \
    testautoplaylist.cpp \
    musiclibscanner.test.cpp \
    ../../src/tags.cpp \
    ../../src/exceptions.cpp \
    musiclibscanner.test.mock.cpp

DEFINES += SRCDIR=\\\"$$PWD/\\\"

HEADERS += \
    ../../src/config.h \
    ../../src/m3uinout.h \
    ../../src/metadataprovider.h \
    ../../src/timeconverter.h \
    ../../src/autoplaylistobject.h \
    ../../src/autoplaylistmanager.h \
    ../../src/autoplaylist.h \
    ../../src/lalatypes.h \
    ../../src/model.h \
    ../../src/musiclibscanner.h \
    lalatest.h \
    test_model.h \
    testautoplaylist.h \
    musiclibscanner.test.h \
    ../../src/tags.h \
    ../../src/exceptions.h \
    ../../src/IDirWalker.h \
    ../../src/IMetaDataProvider.h \
    ../../src/IScannerDB.h \
    musiclibscanner.test.mock.h

unix!macx: {
    INCLUDEPATH += /usr/include/taglib
    LIBS += -ltag
}

macx: {
    INCLUDEPATH += /usr/local/include/taglib
    LIBS += -L/usr/local/lib/ -ltag
}
