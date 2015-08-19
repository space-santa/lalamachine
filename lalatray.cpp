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
    // This is to also quit the app when the main window is closed.
    connect(rootWin_, SIGNAL(quit()), this, SIGNAL(quit()));
    connect(rootWin_,
            SIGNAL(newTitlePlaying(QString)),
            this,
            SLOT(onNewTitlePlaying(QString)));
    setContextMenu(trayIconMenu());
}

void LalaTray::onActivated(ActivationReason reason)
{
    if (rootWin_ == nullptr) {
        qCritical() << "GOT NO WINDOW";
        return;
    }

    // Apparently 'trigger' translates to left-click.
    if (reason == ActivationReason::Trigger) {
        if (rootWin_->property("visible").toBool() == true) {
            QMetaObject::invokeMethod(rootWin_, "hide");
        } else {
            QMetaObject::invokeMethod(rootWin_, "show");
        }
    }
}

void LalaTray::onNewTitlePlaying(const QString &title) { setToolTip(title); }

QMenu *LalaTray::trayIconMenu()
{
    QAction *quitAction = new QAction(QObject::tr("&Quit"), this);
    connect(quitAction, &QAction::triggered, this, &LalaTray::quit);

    QMenu *retval = new QMenu();
    retval->addAction(quitAction);
    return retval;
}
