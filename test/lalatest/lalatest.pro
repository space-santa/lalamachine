#-------------------------------------------------
#
# Project created by QtCreator 2015-02-07T12:00:07
#
#-------------------------------------------------

QT       += testlib qml quick widgets

INCLUDEPATH += /usr/include/taglib

LIBS += -ltag

TARGET = tst_lalatesttest
CONFIG   += console
CONFIG   -= app_bundle

TEMPLATE = app


SOURCES += tst_lalatesttest.cpp \
    ../../config.cpp \
    ../../m3uinout.cpp \
    ../../metadataprovider.cpp \
    ../../timeconverter.cpp
DEFINES += SRCDIR=\\\"$$PWD/\\\"

HEADERS += \
    ../../config.h \
    ../../m3uinout.h \
    ../../metadataprovider.h \
    ../../timeconverter.h
