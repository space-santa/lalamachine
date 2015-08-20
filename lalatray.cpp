#include "lalatray.h"

#include <QDebug>
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

    connect(rootWin_,
            SIGNAL(isPlaying(bool)),
            this,
            SLOT(onPlayingStatusChanged(bool)));
    connect(rootWin_, SIGNAL(setVolume(int)), this, SLOT(onSetVolume(int)));
    connect(this, SIGNAL(volumeUp()), rootWin_, SIGNAL(volumeUp()));
    connect(this, SIGNAL(volumeDown()), rootWin_, SIGNAL(volumeDown()));
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
            if (rootWin_->property("active").toBool() == true) {
            QMetaObject::invokeMethod(rootWin_, "hide");
            } else {
                QMetaObject::invokeMethod(rootWin_, "raise");
                QMetaObject::invokeMethod(rootWin_, "requestActivate");
            }
        } else {
            QMetaObject::invokeMethod(rootWin_, "show");
        }
    }
}

void LalaTray::onNewTitlePlaying(const QString &title) { setToolTip(title); }

void LalaTray::onPlayingStatusChanged(bool stat)
{
    if (stat) {
        playPauseAction_->setText("Pause");
        playPauseAction_->setIcon(QIcon(":/images/images/pause.png"));
    } else {
        playPauseAction_->setText("Play");
        playPauseAction_->setIcon(QIcon(":/images/images/play.png"));
    }
}

void LalaTray::onSetVolume(int val)
{
    if (showVolumeMessage_) {
        showMessage("Volume " + QString::number(val) + "%",
                    toolTip(),
                    QSystemTrayIcon::NoIcon,
                    500);
    }
    showVolumeMessage_ = false;
}

QMenu *LalaTray::trayIconMenu()
{
    QAction *forwardAction = new QAction(QObject::tr("Next"), this);
    forwardAction->setIcon(QIcon(":/images/images/forward.png"));
    connect(forwardAction, SIGNAL(triggered()), rootWin_, SIGNAL(playNext()));

    QAction *backAction = new QAction(QObject::tr("Back"), this);
    backAction->setIcon(QIcon(":/images/images/back.png"));
    connect(backAction, SIGNAL(triggered()), rootWin_, SIGNAL(playPrevious()));

    playPauseAction_ = new QAction(QObject::tr("Play / Pause"), this);
    onPlayingStatusChanged(false);
    connect(playPauseAction_,
            SIGNAL(triggered()),
            rootWin_,
            SIGNAL(playPause()));

    QAction *quitAction = new QAction(QObject::tr("&Quit"), this);
    connect(quitAction, &QAction::triggered, this, &LalaTray::quit);

    QMenu *retval = new QMenu();
    retval->addAction(forwardAction);
    retval->addAction(backAction);
    retval->addAction(playPauseAction_);
    retval->addAction(quitAction);
    return retval;
}

bool LalaTray::event(QEvent *event)
{
    if (event->type() == QEvent::Wheel) {
        showVolumeMessage_ = true;
        QWheelEvent *tmp = (QWheelEvent *)event;
        if (tmp->angleDelta().y() > 0) {
            emit volumeUp();
        } else {
            emit volumeDown();
        }
    }

    return true;
}
