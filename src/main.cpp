// This file is part of lalamachine. License is GPL-3.0 (or later).

#include <QApplication>
#include <QCommandLineParser>
#include <QIcon>
#include <QPixmap>
#include <QQmlApplicationEngine>
#include <QSplashScreen>
#include <memory>

#include "DatabaseSetup.h"
#include "Logger.h"
#include "config.h"
#include "lalatypes.h"
#include "musiclib.h"
#include "playlistmodel.h"
#include "playlistprovider.h"
#include "sysinfo.h"
#include "LalaServer.h"

Logger logger;

void myMessageOutput(QtMsgType type, const QMessageLogContext& context, const QString& msg) {
    QString theMessage = msg + " (" + context.file + ":" + context.line + ", " + context.function + ")";
    if (theMessage.contains("TableView")) {
        // FIXME: This is here to not grow the.log to big. But the underlying problem should be addressed eventually.
        return;
    }
    switch (type) {
        case QtDebugMsg: logger.write(theMessage); break;
        case QtInfoMsg: logger.write(theMessage); break;
        case QtWarningMsg: logger.write(theMessage); break;
        case QtCriticalMsg: logger.write(theMessage); break;
        case QtFatalMsg: logger.write(theMessage); break;
    }
}

void registerQmlTypes() {
    qmlRegisterType<PlaylistProvider>("Lala", 1, 0, "PlaylistProvider");
    qmlRegisterType<Config>("Lala", 1, 0, "Config");
    qmlRegisterType<MusicLib>("Lala", 1, 0, "MusicLib");
    qmlRegisterType<SysInfo>("Lala", 1, 0, "SysInfo");
    qmlRegisterType<LalaTypes>("Lala", 1, 0, "LalaTypes");
    qmlRegisterType<PlaylistModel>("Lala", 1, 0, "PlaylistModel");
}

int main(int argc, char* argv[]) {
    qInstallMessageHandler(myMessageOutput);
    QApplication app(argc, argv);
    app.setWindowIcon(QIcon(QPixmap(":/images/images/lala-icon-2-small.png")));
    app.setApplicationVersion("3.14.0");
    app.setApplicationName("lalamachine");
    app.setOrganizationName("rmean");

    QPixmap logo(":/images/images/logo/logo.png");
    QSplashScreen splash(logo);
    splash.show();

	LalaServer lalaServer;
    if (!lalaServer.start()) {
        qFatal("Couldn't start LalaServer");
	}
    DatabaseSetup databaseSetup;

    registerQmlTypes();
    // INFO: Because we use an ApplicationWindow we cannot use a QQuickView.
    QQmlApplicationEngine engine(QUrl(QStringLiteral("qrc:/qml/main.qml")));

    splash.close();
    return app.exec();
}
