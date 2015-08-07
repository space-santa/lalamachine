#include "lalatray.h"

#include <QDebug>

LalaTray::LalaTray(QObject *root, QObject *parent)
    : QSystemTrayIcon(parent), rootWin_(root)
{
    setIcon(QIcon(QPixmap(":/images/images/lalamachine.png")));
    connect(this, &LalaTray::activated, this, &LalaTray::onActivated);
}

void LalaTray::onActivated()
{
    if (rootWin_ == nullptr) {
        qCritical() << "GOT NO WINDOW";
        return;
    }

    qDebug() << rootWin_->property("visible");

    if (rootWin_->property("visible").toBool() == true) {
        rootWin_->setProperty("visible", false);
    } else {
        rootWin_->setProperty("visible", true);
    }
}
