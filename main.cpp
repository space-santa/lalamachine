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
#include <QQmlApplicationEngine>
#include <QPixmap>
#include <QIcon>
#include <QSplashScreen>

#include "metadataprovider.h"
#include "m3uinout.h"
#include "config.h"
#include "timeconverter.h"
#include "musiclib.h"
#include "cdwriter.h"
#include "sysinfo.h"
#include "lalatypes.h"
#include "autoplaylistmanager.h"
#include "fileexporter.h"
#include "engineloader.h"
#include "playlistmodel.h"
#include "lalatray.h"

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);
    // Setting the app-icon.
    app.setWindowIcon(QIcon(QPixmap(":/images/images/lalamachine.png")));
    app.setApplicationVersion("1.1.1");
    app.setApplicationName("lalamachine");
    app.setOrganizationName("rmean");

    QPixmap logo(":/images/images/logo/logo.png");
    QSplashScreen splash(logo);
    splash.show();

    qmlRegisterType<MetaDataProvider>("Lala", 1, 0, "Metadata");
    qmlRegisterType<M3uInOut>("Lala", 1, 0, "M3uInOut");
    qmlRegisterType<Config>("Lala", 1, 0, "Config");
    qmlRegisterType<TimeConverter>("Lala", 1, 0, "TimeConverter");
    qmlRegisterType<MusicLib>("Lala", 1, 0, "MusicLib");
    qmlRegisterType<CdWriter>("Lala", 1, 0, "CdWriter");
    qmlRegisterType<SysInfo>("Lala", 1, 0, "SysInfo");
    qmlRegisterType<LalaTypes>("Lala", 1, 0, "LalaTypes");
    qmlRegisterType<AutoPlaylistManager>("Lala", 1, 0, "AutoPlaylistManager");
    qmlRegisterType<FileExporter>("Lala", 1, 0, "FileExporter");
    qmlRegisterType<PlaylistModel>("Lala", 1, 0, "PlaylistModel");

    // INFO: Because we use an ApplicationWindow we cannot use a QQuickView.
    EngineLoader loader;

    loader.load();
    splash.close();

    LalaTray tray(loader.rootWin());
    // To be completely honest, there are two reasons why we need this way to
    // quit the app.
    // The main reason is because I actually like this behaviour.
    // Minimize the main window keeps the app in the window bar. Closing the
    // main window doesn't quit the app but makes it run with only the
    // tray icon.
    // The other reason is because I couldn't figure out an easy way to quit the
    // app when closing the main window. (Altough I admittedly didn't put any
    // real effort into it because, as stated above, I like this behaiour.)
    QObject::connect(&tray, &LalaTray::quit, &app, &QApplication::quit);
    tray.show();

    return app.exec();
}
