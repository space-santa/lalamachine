#include "lalatray.h"

#include <QDebug>
#include <QAction>
#include <QMenu>
#include <QApplication>

LalaTray::LalaTray(QObject *root, QObject *parent)
    : QSystemTrayIcon(parent), rootWin_(root)
{
    setIcon(QIcon(QPixmap(":/images/images/lalamachine.png")));
    connect(this, &LalaTray::activated, this, &LalaTray::onActivated);
    setContextMenu(trayIconMenu());
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

QMenu *LalaTray::trayIconMenu()
{
    QAction *quitAction = new QAction(QObject::tr("&Quit"), this);
    connect(quitAction, &QAction::triggered, this, &LalaTray::quit);

    QMenu *retval = new QMenu();
    retval->addAction(quitAction);
    return retval;
}
