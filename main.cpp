#include <QApplication>
#include <QQmlApplicationEngine>
#include <QPixmap>
#include <QIcon>

#include "metadataprovider.h"
#include "m3uinout.h"
#include "config.h"
#include "timeconverter.h"
#include "musiclib.h"
#include "cdwriter.h"

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);
    // Setting the app-icon.
    app.setWindowIcon(QIcon(QPixmap(":/images/images/lalamachine.png")));

    qmlRegisterType<MetaDataProvider>("Lala", 1, 0, "Metadata");
    qmlRegisterType<M3uInOut>("Lala", 1, 0, "M3uInOut");
    qmlRegisterType<Config>("Lala", 1, 0, "Config");
    qmlRegisterType<TimeConverter>("Lala", 1, 0, "TimeConverter");
    qmlRegisterType<MusicLib>("Lala", 1, 0, "MusicLib");
    qmlRegisterType<CdWriter>("Lala", 1, 0, "CdWriter");

    QQmlApplicationEngine engine;
    engine.load(QUrl(QStringLiteral("qrc:/main.qml")));

    return app.exec();
}
