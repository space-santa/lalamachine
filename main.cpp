#include <QApplication>
#include <QQmlApplicationEngine>
#include <QPixmap>
#include <QIcon>

int main(int argc, char *argv[])
{
    QApplication app(argc, argv);
    // Setting the app-icon.
    app.setWindowIcon(QIcon(QPixmap(":/images/images/lalamachine.png")));

    QQmlApplicationEngine engine;
    engine.load(QUrl(QStringLiteral("qrc:/main.qml")));

    return app.exec();
}
