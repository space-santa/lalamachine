#ifndef LALATRAY_H
#define LALATRAY_H

#include <QSystemTrayIcon>
#include <QQuickWindow>
#include <QAction>

class LalaTray : public QSystemTrayIcon
{
    Q_OBJECT
public:
    explicit LalaTray(QObject *root, QObject *parent = 0);

signals:
    void quit();
    void volumeUp();
    void volumeDown();

public slots:

private slots:
    void onActivated(ActivationReason reason);
    void onNewTitlePlaying(const QString &title);
    void onPlayingStatusChanged(bool stat);
    void onSetVolume(int val);

private:
    // The object rootWin_ points to is owned by the qml engine. DO NOT DELETE.
    QObject *rootWin_;
    QMenu *trayIconMenu();
    // We only want to show the volume message if we change the volume here.
    // Don't show the message if the GUI changes the volume.
    bool showVolumeMessage_;

    // This action must be a member beause we need to change its text and icon.
    QAction *playPauseAction_;

    bool event(QEvent *event);
};

#endif  // LALATRAY_H
