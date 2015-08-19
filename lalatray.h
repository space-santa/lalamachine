#ifndef LALATRAY_H
#define LALATRAY_H

#include <QSystemTrayIcon>
#include <QQuickWindow>

class LalaTray : public QSystemTrayIcon
{
    Q_OBJECT
public:

    explicit LalaTray(QObject *root, QObject *parent = 0);

signals:
    void quit();

public slots:

private slots:
    void onActivated(ActivationReason reason);
    void onNewTitlePlaying(const QString &title);

private:
    // The object rootWin_ points to is owned by the qml engine. DO NOT DELETE.
    QObject *rootWin_;

    QMenu *trayIconMenu();
};

#endif  // LALATRAY_H
