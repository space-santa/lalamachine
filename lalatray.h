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

public slots:

private:
    QObject *rootWin_;

private slots:
    void onActivated();
};

#endif  // LALATRAY_H
