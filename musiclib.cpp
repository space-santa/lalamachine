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

#include "musiclib.h"

#include <QMutexLocker>
#include <QDir>
#include <QDirIterator>
#include <QJsonObject>
#include <QUrl>
#include <QSqlQuery>
#include <QSqlError>
#include <QPair>
#include <QElapsedTimer>
#include <QFuture>
#include <QFutureWatcher>
#include <QtConcurrent>

#include "metadataprovider.h"
#include "config.h"
#include "musiclibscanner.h"
#include "autoplaylistmanager.h"

MusicLib::MusicLib(QQuickItem *parent) : QQuickItem(parent)
{
    // This moveToThread is making the thread the parent of the scanner_.
    // Therefor it is vital that the scanner_ is a raw pointer, or double free
    // happens.
    scanner_->moveToThread(&scannerThread_);

    db_ = QSqlDatabase::addDatabase("QSQLITE");
    db_.setDatabaseName(Config::MUSICLIBDB);
    db_.open();
    ensureAllTables();

    connect(&scannerThread_,
            &QThread::finished,
            scanner_,
            &QObject::deleteLater);

    connect(this, &MusicLib::startScan, scanner_, &MusicLibScanner::scanLib);

    connect(scanner_,
            &MusicLibScanner::scanComplete,
            this,
            &MusicLib::scanFinished);

    connect(scanner_,
            &MusicLibScanner::scanStarted,
            this,
            &MusicLib::scanStarted);

    connect(scanner_,
            &MusicLibScanner::trackAdded,
            this,
            &MusicLib::scanUpdate);

    connect(this, &MusicLib::musicLibChanged, this, &MusicLib::setDisplayLib);

    connect(this,
            &MusicLib::titlePartialFilterChanged,
            this,
            &MusicLib::setDisplayLib);

    connect(this,
            &MusicLib::titlePartialFilterChanged,
            this,
            &MusicLib::setGenreList);

    connect(this,
            &MusicLib::titlePartialFilterChanged,
            this,
            &MusicLib::setArtistList);

    connect(this,
            &MusicLib::titlePartialFilterChanged,
            this,
            &MusicLib::setAlbumList);

    // Since setting the genre or artist filter always sets the album filter,
    // we only setDisplayLib here.
    connect(this,
            &MusicLib::albumFilterChanged,
            this,
            &MusicLib::setDisplayLib);

    connect(this, &MusicLib::musicLibChanged, this, &MusicLib::setGenreList);

    connect(this, &MusicLib::musicLibChanged, this, &MusicLib::setArtistList);

    connect(this, &MusicLib::musicLibChanged, this, &MusicLib::setAlbumList);

    connect(this,
            &MusicLib::genreFilterChanged,
            this,
            &MusicLib::setArtistList);

    connect(this, &MusicLib::genreFilterChanged, this, &MusicLib::setAlbumList);

    connect(this,
            &MusicLib::artistFilterChanged,
            this,
            &MusicLib::setAlbumList);

    connect(&watcher_,
            &QFutureWatcher<QSqlQuery>::finished,
            this,
            &MusicLib::onDisplayFutureFinished);

    setGenreList();
}

MusicLib::~MusicLib()
{
    scannerThread_.quit();
    scannerThread_.wait(5000);
    db_.close();
}

const QMap<MusicLib::SortWhat, QString> MusicLib::SORT_MAP = initSortMap();

QMap<MusicLib::SortWhat, QString> MusicLib::initSortMap()
{
    QMap<SortWhat, QString> tmp;
    tmp.insert(MusicLib::ARTIST, "artist");
    tmp.insert(MusicLib::GENRE, "genre");
    tmp.insert(MusicLib::TRACK, "track");
    tmp.insert(MusicLib::TITLE, "title");
    tmp.insert(MusicLib::COMMENT, "comment");
    tmp.insert(MusicLib::LENGTH, "length");

    return tmp;
}

const QString MusicLib::ALL_FILTER{QString("--all--")};

bool MusicLib::scanning() const { return scanning_; }

void MusicLib::setScanning(bool val)
{
    scanning_ = val;
    emit scanningChanged();
}

QJsonArray MusicLib::displayLib() const { return displayLib_; }

int MusicLib::totalLength() const { return totalLength_; }

void MusicLib::debugSignal() { qDebug() << "DEBUGGING SIGNAL"; }

void MusicLib::setDisplayLib()
{
    QString query = getSortQueryString();

    // WARNING: This can cause a problem after a library scan.
    // The LibraryView might not be properly updated once the scan finishes
    // because the query == last query. I am setting lastDisplayLibQuery_ = "-1"
    // in MusicLib::scanFinished() to work around this.
    if (query == lastDisplayLibQuery_) {
        qDebug() << "query didn't change, nothing to do.";
        return;
    }

    lastDisplayLibQuery_ = query;

    auto future = QtConcurrent::run(this, &MusicLib::runSetDisplayQuery, query);
    watcher_.setFuture(future);
}

QSqlQuery MusicLib::runSetDisplayQuery(const QString &query)
{
    QMutexLocker locker(mutex_.data());
    return db_.exec(query);
}

void MusicLib::onDisplayFutureFinished()
{
    auto tmp = queryToJson(watcher_.result());
    displayLib_ = tmp.second;
    emit displayLibChanged();
    totalLength_ = tmp.first;
    emit totalLengthChanged();
}

QString MusicLib::libPath() const { return libPath_; }

void MusicLib::setLibPath(const QString &path)
{
    libPath_ = path;
    emit libPathChanged();

    if (!appStart_) rescan();

    appStart_ = false;
}

QString MusicLib::genreFilter() const { return genreFilter_; }

void MusicLib::setGenreFilter(const QString &val)
{
    // WARNING: We can't return here if genreFilter_ == val because artist and
    // album filter all depend on each other.

    // NOT using setTitlePartialFilter to not emit titlePartialfilterChanged.
    // If that signal would be emitted, everything would be reset.
    titlePartialFilter_ = "";
    if (val == ALL_FILTER) {
        genreFilter_ = "";
    } else {
        genreFilter_ = val;
    }

    // Not using the setter because I only want to setDisplayLib once.
    // If the setter for artistFilter is used, it will trigger it again.
    setArtistFilter("");
    emit genreFilterChanged();
}

QString MusicLib::artistFilter() const { return artistFilter_; }

void MusicLib::setArtistFilter(const QString &val)
{
    // See comment in setGenreFilter()
    titlePartialFilter_ = "";
    if (val == ALL_FILTER) {
        artistFilter_ = "";
    } else {
        artistFilter_ = val;
    }

    // See comment in setGenreFilter()
    setAlbumFilter("");
    emit artistFilterChanged();
}

QString MusicLib::albumFilter() const { return albumFilter_; }

void MusicLib::setAlbumFilter(const QString &val)
{
    // See comment in setGenreFilter()
    titlePartialFilter_ = "";
    if (val == ALL_FILTER) {
        albumFilter_ = "";
    } else {
        albumFilter_ = val;
    }

    emit albumFilterChanged();
}

bool MusicLib::sortAsc() const { return sortAsc_; }

void MusicLib::setSortAsc(bool val)
{
    if (sortAsc_ == val) return;
    // Only do things when there was an actual change.
    // We only want to setDisplayLib if necessary.
    sortAsc_ = val;
    emit sortAscChanged();
    setDisplayLib();
}

MusicLib::SortWhat MusicLib::what() const { return what_; }

void MusicLib::setWhat(MusicLib::SortWhat val)
{
    if (what_ == val) return;
    // See comment in MusicLib::setSortAsc
    what_ = val;
    emit whatChanged();
    setDisplayLib();
}

QStringList MusicLib::genreList() const { return genreList_; }

QStringList MusicLib::artistList() const { return artistList_; }

QStringList MusicLib::albumList() const { return albumList_; }

void MusicLib::rescan()
{
    qDebug() << "emitting rescan" << libPath();

    if (!scannerThread_.isRunning()) {
        scannerThread_.start();
    }

    clearMusicLib();

    emit startScan(libPath());
}

QJsonArray MusicLib::autoPlaylist(const QJsonArray &json)
{
    auto args = AutoPlaylistManager::jsonToApo(json);
    // SELECT * FROM musiclib WHERE Tag Op Val
    QString query("SELECT * FROM musiclib WHERE ");

    int count = 0;

    for (auto itr = args.begin(); itr != args.end(); ++itr) {
        if (count > 0) {
            query.append(" ");
            query.append(LalaTypes::ANDOR_MAP.value((*itr).andor()));
            query.append(" ");
        }

        query.append(LalaTypes::TAG_MAP.value((*itr).tag()));
        query.append(" ");
        query.append(LalaTypes::OP_MAP.value((*itr).op()));

        QString tmp("");

        if ((*itr).op() == LalaTypes::CONTAINS
            || (*itr).op() == LalaTypes::CONTAINS_NOT) {
            tmp = " '%%1%'";
        } else {
            tmp = " '%1'";
        }

        query.append(tmp.arg(escapeString((*itr).val())));

        ++count;
    }

    query.append(" ORDER BY artist, album, track");
    qDebug() << query;
    QMutexLocker locker(mutex_.data());
    auto result = db_.exec(query);

    return queryToJson(result).second;
}

void MusicLib::resetFilterAndSort()
{
    // Not using the setter functions because I only want to setDisplayLib once.
    // Otherwise it will take twice as long for this to return.
    sortAsc_ = true;
    what_ = MusicLib::ARTIST;

    genreFilter_ = "";
    artistFilter_ = "";
    albumFilter_ = "";
    setTitlePartialFilter("");
}

QJsonArray MusicLib::getAlbumTracks(const QString &album)
{
    QString query("SELECT * FROM musiclib WHERE album = '%1' ORDER BY track");

    QMutexLocker locker(mutex_.data());
    QSqlQuery result = db_.exec(query.arg(album));

    return queryToJson(result).second;
}

QString MusicLib::escapeString(QString str)
{
    // return str.replace("\'", "\'\'").replace(",", "\'+\',\'+\'");
    return str.replace("\'", "\'\'");
}

QStringList MusicLib::getList(const QString &what) const
{
    QStringList retval;
    QSqlQuery result;

    QMutexLocker locker(mutex_.data());

    if (what == "genre") {
        result = db_.exec(getGenreListQuery());
    } else if (what == "artist") {
        result = db_.exec(getArtistListQuery());
    } else if (what == "album") {
        result = db_.exec(getAlbumListQuery());
    } else {
        qFatal("No valid filter!");
    }

    while (result.next()) {
        QString tmp = result.value(what).toString();

        if (tmp != "") {
            retval << tmp;
        }
    }

    return retval;
}

QString MusicLib::getSortQueryString() const
{
    QString query("SELECT * FROM musiclib WHERE title NOT NULL ");

    if (!titlePartialFilter().isEmpty()) {
        query.append("AND UPPER(title) LIKE '%");
        query.append(escapeString(titlePartialFilter().toUpper()));
        query.append("%' ");
    } else {
        if (!genreFilter().isEmpty()) {
            query.append("AND genre = '");
            query.append(escapeString(genreFilter()));
            query.append("' ");
        }

        if (!artistFilter().isEmpty()) {
            query.append("AND artist = '");
            query.append(escapeString(artistFilter()));
            query.append("'");
        }

        if (!albumFilter().isEmpty()) {
            query.append("AND album = '");
            query.append(escapeString(albumFilter()));
            query.append("'");
        }
    }

    query.append(" ORDER BY ");
    query.append(SORT_MAP.value(what()));
    query.append(" ");

    if (sortAsc()) {
        query.append("ASC");
    } else {
        query.append("DESC");
    }

    query.append(", artist, album, track");

    qDebug() << "ZZZ" << query;
    return query;
}

QString MusicLib::getGenreListQuery() const
{
    QString query("SELECT DISTINCT genre FROM musiclib %1 ORDER BY genre ASC");

    if (!titlePartialFilter().isEmpty()) {
        query = query.arg("WHERE UPPER(genre) LIKE '%%1%'")
                    .arg(escapeString(titlePartialFilter().toUpper()));
    } else {
        query = query.arg("");
    }

    return query;
}

QString MusicLib::getArtistListQuery() const
{
    QString query(
        "SELECT DISTINCT artist FROM musiclib WHERE artist NOT NULL ");

    if (!titlePartialFilter().isEmpty()) {
        query.append("AND UPPER(artist) LIKE '%");
        query.append(escapeString(titlePartialFilter().toUpper()));
        query.append("%' ");
    } else {
        if (!genreFilter().isEmpty()) {
            query.append("AND genre = '");
            query.append(escapeString(genreFilter()));
            query.append("' ");
        }
    }

    query.append(" ORDER BY artist ASC");

    return query;
}

QString MusicLib::getAlbumListQuery() const
{
    QString query("SELECT DISTINCT album FROM musiclib WHERE album NOT NULL ");

    if (!titlePartialFilter().isEmpty()) {
        query.append("AND UPPER(album) LIKE '%");
        query.append(escapeString(titlePartialFilter().toUpper()));
        query.append("%' ");
    } else {
        if (!genreFilter().isEmpty()) {
            query.append("AND genre = '");
            query.append(escapeString(genreFilter()));
            query.append("' ");
        }

        if (!artistFilter().isEmpty()) {
            query.append("AND artist = '");
            query.append(escapeString(artistFilter()));
            query.append("'");
        }
    }

    query.append(" ORDER BY album ASC");

    return query;
}

void MusicLib::ensureAllTables()
{
    auto tables = db_.tables();

    if (!tables.contains("musiclib")) {
        QString qs("CREATE TABLE `musiclib` ");
        qs.append("(\n");
        // qs.append("`ID` INTEGER NOT NULL AUTOINCREMENT,\n");
        qs.append("`album` TEXT,\n");
        qs.append("`artist` TEXT,\n");
        qs.append("`comment` TEXT,\n");
        qs.append("`genre` TEXT,\n");
        qs.append("`length` int NOT NULL,\n");
        qs.append("`lengthString` TEXT NOT NULL,\n");
        qs.append("`mrl` TEXT NOT NULL PRIMARY KEY,\n");
        qs.append("`path` TEXT NOT NULL,\n");
        qs.append("`title` TEXT NOT NULL,\n");
        qs.append("`track` int,\n");
        qs.append("`year` int\n");
        qs.append(")");

        QMutexLocker locker(mutex_.data());
        qDebug() << db_.exec(qs).lastError();
    }
}

void MusicLib::clearMusicLib()
{
    QString query("DELETE FROM musiclib");
    QMutexLocker locker(mutex_.data());
    qDebug() << db_.exec(query).lastError();
}

QPair<int, QJsonArray> MusicLib::queryToJson(QSqlQuery result) const
{
    QJsonArray retVal;

    int totalLength = 0;

    while (result.next()) {
        QJsonObject tmp;

        int len = result.value("length").toInt();
        totalLength += len;

        tmp.insert("album", result.value("album").toString());
        tmp.insert("artist", result.value("artist").toString());
        tmp.insert("genre", result.value("genre").toString());
        tmp.insert("comment", result.value("comment").toString());
        tmp.insert("track", result.value("track").toInt());
        tmp.insert("title", result.value("title").toString());
        tmp.insert("mrl", result.value("mrl").toString());
        tmp.insert("path", result.value("path").toString());
        tmp.insert("length", len);
        tmp.insert("lengthString", result.value("lengthString").toString());
        tmp.insert("year", result.value("year").toInt());

        retVal.append(tmp);
    }

    return QPair<int, QJsonArray>(totalLength, retVal);
}

void MusicLib::setGenreList()
{
    genreList_ = getList("genre");
    genreList_.sort();
    genreList_.prepend(ALL_FILTER);
    emit genreListChanged();
}

void MusicLib::setArtistList()
{
    artistList_ = getList("artist");
    artistList_.sort();
    artistList_.prepend(ALL_FILTER);
    emit artistListChanged();
}

void MusicLib::setAlbumList()
{
    albumList_ = getList("album");
    albumList_.sort();
    albumList_.prepend(ALL_FILTER);
    emit albumListChanged();
}

void MusicLib::scanStarted() { setScanning(true); }

void MusicLib::scanUpdate()
{
    lastDisplayLibQuery_ = "";
    setDisplayLib();
    setGenreList();
    setArtistList();
    setAlbumList();

    // FIXME 1.x: I need a way to update the lists without changing the
    // selection.
    // Probably the setDisplayLib query must run in a separate thread and
    // add line by line to the display lib.
    // Perhaps the same is true for the other lists.
    // Or, since this shouldn't happen too often, I don't care about it.
    // I come back to this when I rescan the library daily.
}

void MusicLib::scanFinished()
{
    // Setting the query to something invalid to have the check in setDisplayLib
    // do the right thing and display stuff as expected.
    // FIXME: Having to do that makes me feel dirty. Is the concept sound?
    lastDisplayLibQuery_ = "-1";
    emit musicLibChanged();
    setScanning(false);
}

QString MusicLib::titlePartialFilter() const { return titlePartialFilter_; }
void MusicLib::setTitlePartialFilter(const QString &titlePartialFilter)
{
    titlePartialFilter_ = titlePartialFilter;
    emit titlePartialFilterChanged();
}

bool MusicLib::checkVal(const QString &check, const QString &val) const
{
    return check == "" || check == val;
}
