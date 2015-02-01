#include <QApplication>
#include <QQmlApplicationEngine>
#include <QPixmap>
#include <QIcon>

#include "metadataprovider.h"
#include "m3uinout.h"

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);
    // Setting the app-icon.
    app.setWindowIcon(QIcon(QPixmap(":/images/images/lalamachine.png")));

    qmlRegisterType<MetaDataProvider>("Lala", 1, 0, "Metadata");
    qmlRegisterType<M3uInOut>("Lala", 1, 0, "M3uInOut");

    QQmlApplicationEngine engine;
    engine.load(QUrl(QStringLiteral("qrc:/main.qml")));

    return app.exec();
}
