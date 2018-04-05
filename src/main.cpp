/*
Copyright 2015 Armin Zirkel

This file is part of lalamachine.

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
*/

#include <QApplication>
#include <QCommandLineParser>
#include <QIcon>
#include <QPixmap>
#include <QQmlApplicationEngine>
#include <QSplashScreen>
#include <memory>

#include "autoplaylistmanager.h"
#include "config.h"
#include "fileexporter.h"
#include "lalatypes.h"
#include "m3uinout.h"
#include "metadataprovider.h"
#include "musiclib.h"
#include "playlistmodel.h"
#include "sysinfo.h"
#include "theplayer.h"
#include "timeconverter.h"

void registerQmlTypes() {
    qmlRegisterType<MetaDataProvider>("Lala", 1, 0, "Metadata");
    qmlRegisterType<M3uInOut>("Lala", 1, 0, "M3uInOut");
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
    app.setApplicationVersion("3.5.2");
    app.setApplicationName("lalamachine");
    app.setOrganizationName("rmean");

    QPixmap logo(":/images/images/logo/logo.png");
    QSplashScreen splash(logo);
    splash.show();

    registerQmlTypes();
    // INFO: Because we use an ApplicationWindow we cannot use a QQuickView.
    QQmlApplicationEngine engine(QUrl(QStringLiteral("qrc:/main.qml")));

    splash.close();
    return app.exec();
}
