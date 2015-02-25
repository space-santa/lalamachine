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

    QJsonObject musicLib() const {
        return lib_;
    }

    QJsonObject displayLib() const;

    QString libPath() const {
        return libPath_;
    }
    void setLibPath(const QString &path) {
        libPath_ = path;
        emit libPathChanged();
        emit startScan(libPath_);
    }

    QString genre() const {
        return genre_;
    }
    void setGenre(const QString &val) {
        genre_ = val;
        emit genreChanged();
        emit displayLibChanged();
    }
    QString artist() const {
        return artist_;
    }
    void setArtist(const QString &val) {
        artist_ = val;
        emit artistChanged();
        emit displayLibChanged();
    }
    QString album() const {
        return album_;
    }
    void setAlbum(const QString &val) {
        genre_ = val;
        emit albumChanged();
        emit displayLibChanged();
    }

public slots:
    void scanFinished(const QJsonObject &lib);

signals:
    void startScan(const QString &path);
    void musicLibChanged();

    void displayLibChanged();

    void libPathChanged();
    void genreChanged();
    void artistChanged();
    void albumChanged();

private:
    // scanner_ MUST be a raw pointer. When this is moved to a new thread, that
    // QThread becomes the parent. When the parent dies so does the child.
    // If this is then not a raw pointer a double free happens, because
    // the thread and this is trying to destroy the scanner.
    MusicLibScanner *scanner_{new MusicLibScanner()};
    QThread scannerThread_;

    QJsonObject lib_;
    QString genre_{""};
    QString artist_{""};
    QString album_{""};
    QString libPath_{""};

    bool checkVal(const QString &check, const QString &val) const;

private slots:
    void readLibFile();
    void writeLibFile();
};

#endif // MUSICLIB_H
