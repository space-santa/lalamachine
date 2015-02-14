#ifndef MUSICLIBSCANNER_H
#define MUSICLIBSCANNER_H

#include <QThread>
#include <QJsonObject>
#include <QString>
#include <QQuickItem>
#include <QMutex>

class MusicLibScanner : public QObject
{
    Q_OBJECT

public:
    MusicLibScanner(QObject *parent = 0);

public slots:
    void scanLib(const QString &path);

signals:
    void scanComplete(const QJsonObject &lib);

private:
    QMutex mutex_;
    bool suffixCheck(const QString &val) const;
};

class MusicLib : public QQuickItem
{
    Q_OBJECT

    Q_PROPERTY(QJsonObject musicLib
               READ musicLib
               NOTIFY musicLibChanged)

public:
    MusicLib(QQuickItem *parent = 0);
    ~MusicLib();

    QJsonObject musicLib() {
        return lib_;
    }

public slots:
    void scanFinished(const QJsonObject &lib);

signals:
    void startScan(const QString &path);
    void musicLibChanged();

private:
    MusicLibScanner *scanner_;
    QThread *scannerThread_;

    QJsonObject lib_;
};

#endif // MUSICLIB_H
