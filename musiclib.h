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

    Q_PROPERTY(QJsonObject displayLib
               READ displayLib
               NOTIFY displayLibChanged)

    Q_PROPERTY(QString libPath
               READ libPath
               WRITE setLibPath
               NOTIFY libPathChanged)

    Q_PROPERTY(QString genreFilter
               READ genreFilter
               WRITE setGenreFilter
               NOTIFY genreFilterChanged)
    Q_PROPERTY(QString artistFilter
               READ artistFilter
               WRITE setArtistFilter
               NOTIFY artistFilterChanged)
    Q_PROPERTY(QString albumFilter
               READ albumFilter
               WRITE setAlbumFilter
               NOTIFY albumFilterChanged)

    Q_PROPERTY(QStringList genreList
               READ genreList
               NOTIFY genreListChanged)
    Q_PROPERTY(QStringList artistList
               READ artistList
               NOTIFY artistListChanged)
    Q_PROPERTY(QStringList albumList
               READ albumList
               NOTIFY albumListChanged)

public:
    MusicLib(QQuickItem *parent = 0);
    ~MusicLib();

    QJsonObject musicLib() const;

    QJsonObject displayLib() const;

    QString libPath() const;
    void setLibPath(const QString &path);

    QString genreFilter() const;
    void setGenreFilter(const QString &val);

    QString artistFilter() const;
    void setArtistFilter(const QString &val);

    QString albumFilter() const;
    void setAlbumFilter(const QString &val);

    QStringList genreList() const;
    QStringList artistList() const;
    QStringList albumList() const;

public slots:
    void scanFinished(const QJsonObject &lib);

signals:
    void startScan(const QString &path);
    void musicLibChanged();

    void displayLibChanged();

    void libPathChanged();
    void genreFilterChanged();
    void artistFilterChanged();
    void albumFilterChanged();

    void genreListChanged();
    void artistListChanged();
    void albumListChanged();

    void displayListChanged();

private:
    // scanner_ MUST be a raw pointer. When this is moved to a new thread, that
    // QThread becomes the parent. When the parent dies so does the child.
    // If this is then not a raw pointer a double free happens, because
    // the thread and this is trying to destroy the scanner.
    MusicLibScanner *scanner_ {new MusicLibScanner()};
    QThread scannerThread_;

    QJsonObject lib_;
    QString genreFilter_ {""};
    QString artistFilter_ {""};
    QString albumFilter_ {""};
    QString libPath_ {""};
    QStringList genreList_ {};
    QStringList artistList_ {};
    QStringList albumList_ {};

    bool checkVal(const QString &check, const QString &val) const;

    QStringList getList(const QString &what) const;

private slots:
    void readLibFile();
    void writeLibFile();
};

#endif // MUSICLIB_H
