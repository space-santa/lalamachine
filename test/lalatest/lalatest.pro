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
    ../../config.cpp \
    ../../m3uinout.cpp \
    ../../metadataprovider.cpp \
    ../../timeconverter.cpp \
    ../../cdwriter.cpp \
    ../../musiclib.cpp \
    ../../systeminteractor.cpp \
    ../../autoplaylistobject.cpp \
    ../../autoplaylistmanager.cpp \
    ../../autoplaylist.cpp \
    ../../lalatypes.cpp \
    ../../musiclibscanner.cpp \
    main.cpp \
    lalatest.cpp \
    testautoplaylist.cpp \
    ../../tags.cpp
DEFINES += SRCDIR=\\\"$$PWD/\\\"

HEADERS += \
    ../../config.h \
    ../../m3uinout.h \
    ../../metadataprovider.h \
    ../../timeconverter.h \
    ../../cdwriter.h \
    ../../musiclib.h \
    ../../systeminteractor.h \
    ../../autoplaylistobject.h \
    ../../autoplaylistmanager.h \
    ../../autoplaylist.h \
    ../../lalatypes.h \
    ../../musiclibscanner.h \
    lalatest.h \
    testautoplaylist.h \
    ../../tags.h
