#include "mainwindow.h"
#include <QQuickWidget>

MainWindow::MainWindow(QWidget* parent, Qt::WindowFlags flags) : QMainWindow(parent, flags) {
    engine_ = new QQmlApplicationEngine();
    engine_->load(QUrl(QStringLiteral("qrc:/main.qml")));
    QQuickWidget* cw = new QQuickWidget(engine_, this);
    setCentralWidget(cw);
    centralWidget()->setMinimumSize(500, 500);
}
