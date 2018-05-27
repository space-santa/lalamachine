TEMPLATE = app

QT += qml quick widgets sql concurrent multimedia quickwidgets

CONFIG += C++11 # console

win32:RC_ICONS += images/lalamachine-new.ico

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
    src/playlistprovider.cpp \
    src/DatabaseSetup.cpp \
    src/QmlMetadataProvider.cpp

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
    src/playlistprovider.h \
    src/DatabaseSetup.h \
    src/QmlMetadataProvider.h

OTHER_FILES += \
    README.md \
    images/license-icons.txt \
    lalamachine.desktop \
    lalamachine.spec

DISTFILES += \
    LICENSE \
    changelog \
    package.json

ICON = ./images/lala-icon.png.icns

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
    LIBS += -L$$PWD/../../../lib/taglib/lib/ -llibtag.dll
    INCLUDEPATH += $$PWD/../../../lib/taglib/include/taglib
    DEPENDPATH += $$PWD/../../../lib/taglib/include/taglib
}

win32-msvc* {
    LIBS += -L$$PWD/../../../Libraries/taglib/lib/ -ltag
    INCLUDEPATH += $$PWD/../../../Libraries/taglib/include/taglib
    DEPENDPATH += $$PWD/../../../Libraries/taglib/include
}
