// This file is part of lalamachine. License is GPL-3.0 (or later).

#include <QApplication>
#include <QCommandLineParser>
#include <QIcon>
#include <QPixmap>
#include <QQmlApplicationEngine>
#include <QSplashScreen>
#include <memory>

#include "DatabaseSetup.h"
#include "QmlMetadataProvider.h"
#include "autoplaylistmanager.h"
#include "config.h"
#include "fileexporter.h"
#include "lalatypes.h"
#include "musiclib.h"
#include "playlistmodel.h"
#include "playlistprovider.h"
#include "sysinfo.h"
#include "theplayer.h"
#include "timeconverter.h"

void registerQmlTypes() {
    qmlRegisterType<QmlMetadataProvider>("Lala", 1, 0, "Metadata");
    qmlRegisterType<PlaylistProvider>("Lala", 1, 0, "PlaylistProvider");
    qmlRegisterType<Config>("Lala", 1, 0, "Config");
    qmlRegisterType<TimeConverter>("Lala", 1, 0, "TimeConverter");
    qmlRegisterType<MusicLib>("Lala", 1, 0, "MusicLib");
    qmlRegisterType<SysInfo>("Lala", 1, 0, "SysInfo");
    qmlRegisterType<LalaTypes>("Lala", 1, 0, "LalaTypes");
    qmlRegisterType<AutoPlaylistManager>("Lala", 1, 0, "AutoPlaylistManager");
    qmlRegisterType<FileExporter>("Lala", 1, 0, "FileExporter");
    qmlRegisterType<PlaylistModel>("Lala", 1, 0, "PlaylistModel");
    qmlRegisterType<ThePlayer>("Lala", 1, 0, "ThePlayer");
}

int main(int argc, char* argv[]) {
    QApplication app(argc, argv);
    app.setWindowIcon(QIcon(QPixmap(":/images/images/lala-icon-2-small.png")));
    app.setApplicationVersion("3.6.1");
    app.setApplicationName("lalamachine");
    app.setOrganizationName("rmean");

    QPixmap logo(":/images/images/logo/logo.png");
    QSplashScreen splash(logo);
    splash.show();

    DatabaseSetup databaseSetup;

    registerQmlTypes();
    // INFO: Because we use an ApplicationWindow we cannot use a QQuickView.
    QQmlApplicationEngine engine(QUrl(QStringLiteral("qrc:/main.qml")));

    splash.close();
    return app.exec();
}
