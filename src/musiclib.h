/*
Copyright 2015 Armin Zirkel

This file is part of lalamachine.

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
*/

#ifndef MUSICLIB_H
#define MUSICLIB_H

#include <QThread>
#include <QJsonObject>
#include <QJsonArray>
#include <QString>
#include <QObject>
#include <QMutex>
#include <QSharedPointer>
#include <QSqlDatabase>
#include <QFutureWatcher>

#include "musiclibscanner.h"

class MusicLib : public QObject
{
    Q_OBJECT

    // clang-format off
    Q_PROPERTY(bool scanning
               READ scanning
               WRITE setScanning
               NOTIFY scanningChanged)

    Q_PROPERTY(QJsonArray displayLib
               READ displayLib
               NOTIFY displayLibChanged)

    Q_PROPERTY(int totalLength
               READ totalLength
               NOTIFY totalLengthChanged)

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

    Q_PROPERTY(bool sortAsc READ sortAsc WRITE setSortAsc NOTIFY sortAscChanged)
    Q_PROPERTY(SortWhat what READ what WRITE setWhat NOTIFY whatChanged)

    Q_PROPERTY(QString titlePartialFilter
               READ titlePartialFilter
               WRITE setTitlePartialFilter
               NOTIFY titlePartialFilterChanged)
    // clang-format on

public:
    enum SortWhat {
        TRACK,
        TITLE,
        COMMENT,
        LENGTH,
        GENRE,
        ARTIST,
        ALBUM,
        DATEADDED
    };
    Q_ENUMS(SortWhat)
    enum SortHow { ASCENDING, DESCENDING };
    Q_ENUMS(SortHow)

    MusicLib(QObject *parent = 0);
    ~MusicLib();

    static const QString ALL_FILTER;
    static const QMap<SortWhat, QString> SORT_MAP;
    static QMap<SortWhat, QString> initSortMap();

    bool scanning() const;
    void setScanning(bool val);

    QJsonArray displayLib() const;
    int totalLength() const;

    QString libPath() const;
    void setLibPath(const QString &path);

    QString genreFilter() const;
    void setGenreFilter(const QString &val);

    QString artistFilter() const;
    void setArtistFilter(const QString &val);

    QString albumFilter() const;
    void setAlbumFilter(const QString &val);

    bool sortAsc() const;
    void setSortAsc(bool val);

    SortWhat what() const;
    void setWhat(SortWhat val);

    QStringList genreList() const;
    QStringList artistList() const;
    QStringList albumList() const;

    Q_INVOKABLE void rescan();

    Q_INVOKABLE void resetFilterAndSort();
    Q_INVOKABLE QJsonArray getAlbumTracks(const QString &album);

    Q_INVOKABLE QString getDateAddedByMrl(const QString &mrl) const;

    Q_INVOKABLE QJsonObject getMetadataForMrl(const QString &mrl) const;

    static QString escapeString(QString str);

    static QPair<int, QJsonArray> queryResultToJson(QSqlQuery result);

    QString titlePartialFilter() const;
    void setTitlePartialFilter(const QString &titlePartialFilter);

    static QString cleanPath(QString mrl);
public slots:
    void scanFinished();

signals:
    void startScan(const QString &path);
    void musicLibChanged();

    void scanningChanged();

    void displayLibChanged();
    void totalLengthChanged();

    void libPathChanged();
    void genreFilterChanged();
    void artistFilterChanged();
    void albumFilterChanged();

    void genreListChanged();
    void artistListChanged();
    void albumListChanged();

    void sortAscChanged();
    void whatChanged();

    void titlePartialFilterChanged();

private:
    // scanner_ MUST be a raw pointer. When this is moved to a new thread, that
    // QThread becomes the parent. When the parent dies so does the child.
    // If this is then not a raw pointer a double free happens, because
    // the thread and this is trying to destroy the scanner.
    MusicLibScanner *scanner_;
    QThread scannerThread_;

    QSharedPointer<QMutex> mutex_;
    QSqlDatabase db_;
    bool sortAsc_;
    bool scanning_;
    SortWhat what_;
    QJsonArray displayLib_;
    int totalLength_;
    QString genreFilter_;
    QString artistFilter_;
    QString albumFilter_;
    QString libPath_;
    QStringList genreList_;
    QStringList artistList_;
    QStringList albumList_;
    QString titlePartialFilter_;
    bool appStart_;
    QString lastDisplayLibQuery_;

    QFutureWatcher<QSqlQuery> watcher_;

    bool checkVal(const QString &check, const QString &val) const;

    QStringList getList(const QString &what) const;

    QString getSortQueryString() const;
    QString getGenreListQuery() const;
    QString getArtistListQuery() const;
    QString getAlbumListQuery() const;

    void ensureAllTables();
    void clearMusicLib();

    QSqlQuery runSetDisplayQuery(const QString &query);

    void updateTable();

    void createLibTable(const QString &name);
    void copyLibToTmp();
    void restoreMetaData();
    void init();
private slots:
    void debugSignal();

    void setDisplayLib();
    void setGenreList();
    void setArtistList();
    void setAlbumList();

    void scanStarted();

    void scanUpdate();
    void onDisplayFutureFinished();
};

#endif  // MUSICLIB_H