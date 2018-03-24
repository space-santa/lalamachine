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

#include <QDir>
#include <QDirIterator>
#include <QElapsedTimer>
#include <QFuture>
#include <QFutureWatcher>
#include <QJsonObject>
#include <QMutexLocker>
#include <QPair>
#include <QSqlError>
#include <QSqlQuery>
#include <QSqlRecord>
#include <QUrl>
#include <QtConcurrent>

#include "autoplaylistmanager.h"
#include "config.h"
#include "metadataprovider.h"
#include "model.h"
#include "musiclibscanner.h"

MusicLib::MusicLib(QObject* parent) : QObject(parent) {
    init();
    // This moveToThread is making the thread the parent of the scanner_.
    // Therefor it is vital that the scanner_ is a raw pointer, or double free
    // happens.
    scanner_->moveToThread(&scannerThread_);

    connect(&scannerThread_, &QThread::finished, scanner_, &QObject::deleteLater);

    connect(this, &MusicLib::startScan, scanner_, &MusicLibScanner::scanLib);

    connect(scanner_, &MusicLibScanner::scanComplete, this, &MusicLib::scanFinished);

    connect(scanner_, &MusicLibScanner::scanStarted, this, &MusicLib::scanStarted);

    connect(scanner_, &MusicLibScanner::trackAdded, this, &MusicLib::scanUpdate);

    connect(this, &MusicLib::musicLibChanged, this, &MusicLib::setDisplayLib);

    connect(this, &MusicLib::titlePartialFilterChanged, this, &MusicLib::setDisplayLib);

    connect(this, &MusicLib::titlePartialFilterChanged, this, &MusicLib::setGenreList);

    connect(this, &MusicLib::titlePartialFilterChanged, this, &MusicLib::setArtistList);

    connect(this, &MusicLib::titlePartialFilterChanged, this, &MusicLib::setAlbumList);

    // Since setting the genre or artist filter always sets the album filter,
    // we only setDisplayLib here.
    connect(this, &MusicLib::albumFilterChanged, this, &MusicLib::setDisplayLib);

    connect(this, &MusicLib::musicLibChanged, this, &MusicLib::setGenreList);

    connect(this, &MusicLib::musicLibChanged, this, &MusicLib::setArtistList);

    connect(this, &MusicLib::musicLibChanged, this, &MusicLib::setAlbumList);

    connect(this, &MusicLib::genreFilterChanged, this, &MusicLib::setArtistList);

    connect(this, &MusicLib::genreFilterChanged, this, &MusicLib::setAlbumList);

    connect(this, &MusicLib::artistFilterChanged, this, &MusicLib::setAlbumList);

    connect(&watcher_, &QFutureWatcher<QSqlQuery>::finished, this, &MusicLib::onDisplayFutureFinished);

    setGenreList();
}

MusicLib::~MusicLib() {
    scannerThread_.quit();
    scannerThread_.wait(5000);
}

void MusicLib::init() {
    scanner_ = new MusicLibScanner();
    mutex_ = QSharedPointer<QMutex>(new QMutex());
    sortAsc_ = true;
    scanning_ = false;
    what_ = Model::ARTIST;
    totalLength_ = 0;
    genreFilter_ = "";
    artistFilter_ = "";
    albumFilter_ = "";
    libPath_ = "";
    genreList_.clear();
    artistList_.clear();
    albumList_.clear();
    titlePartialFilter_ = "";
    appStart_ = true;
    lastDisplayLibQuery_ = "";
}

const QString MusicLib::ALL_FILTER = QString("--all--");

bool MusicLib::scanning() const {
    return scanning_;
}

void MusicLib::setScanning(bool val) {
    scanning_ = val;
    emit scanningChanged();
}

QJsonArray MusicLib::displayLib() const {
    return displayLib_;
}

int MusicLib::totalLength() const {
    return totalLength_;
}

void MusicLib::debugSignal() {
    qDebug() << "DEBUGGING SIGNAL";
}

void MusicLib::setDisplayLib() {
    QString query = Model::getSortQueryString(titlePartialFilter(),
                                              genreFilter(),
                                              artistFilter(),
                                              albumFilter(),
                                              what(),
                                              sortAsc());

    // WARNING: This can cause a problem after a library scan.
    // The LibraryView might not be properly updated once the scan finishes
    // because the query == last query. I am setting lastDisplayLibQuery_ = "-1"
    // in MusicLib::scanFinished() to work around this.
    if (query == lastDisplayLibQuery_) {
        qDebug() << "query didn't change, nothing to do.";
        return;
    }

    lastDisplayLibQuery_ = query;

    QFuture<QPair<int, QJsonArray>> future = QtConcurrent::run(Model::instance(), &Model::runSetDisplayQuery, query);
    watcher_.setFuture(future);
}

void MusicLib::onDisplayFutureFinished() {
    QPair<int, QJsonArray> tmp = watcher_.result();
    displayLib_ = tmp.second;
    emit displayLibChanged();
    totalLength_ = tmp.first;
    emit totalLengthChanged();
}

QString MusicLib::libPath() const {
    return libPath_;
}

void MusicLib::setLibPath(const QString& path) {
    libPath_ = path;
    emit libPathChanged();

    if (!appStart_) { rescan(); }

    appStart_ = false;
}

QString MusicLib::genreFilter() const {
    return genreFilter_;
}

void MusicLib::setGenreFilter(const QString& val) {
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

QString MusicLib::artistFilter() const {
    return artistFilter_;
}

void MusicLib::setArtistFilter(const QString& val) {
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

QString MusicLib::albumFilter() const {
    return albumFilter_;
}

void MusicLib::setAlbumFilter(const QString& val) {
    // See comment in setGenreFilter()
    titlePartialFilter_ = "";
    if (val == ALL_FILTER) {
        albumFilter_ = "";
    } else {
        albumFilter_ = val;
    }

    emit albumFilterChanged();
}

bool MusicLib::sortAsc() const {
    return sortAsc_;
}

void MusicLib::setSortAsc(bool val) {
    if (sortAsc_ == val) { return; }

    // Only do things when there was an actual change.
    // We only want to setDisplayLib if necessary.
    sortAsc_ = val;
    emit sortAscChanged();
    setDisplayLib();
}

Model::SortWhat MusicLib::what() const {
    return what_;
}

void MusicLib::setWhat(Model::SortWhat val) {
    if (what_ == val) { return; }
    // See comment in MusicLib::setSortAsc
    what_ = val;
    emit whatChanged();
    setDisplayLib();
}

QStringList MusicLib::genreList() const {
    return genreList_;
}

QStringList MusicLib::artistList() const {
    return artistList_;
}

QStringList MusicLib::albumList() const {
    return albumList_;
}

void MusicLib::resetFilterAndSort() {
    // Not using the setter functions because I only want to setDisplayLib once.
    // Otherwise it will take twice as long for this to return.
    sortAsc_ = true;
    what_ = Model::ARTIST;

    genreFilter_ = "";
    artistFilter_ = "";
    albumFilter_ = "";
    setTitlePartialFilter("");
}

QStringList MusicLib::getList(const QString& what) const {
    QStringList retval;

    if (what == "genre") {
        retval = Model::instance()->getGenreList(titlePartialFilter());
    } else if (what == "artist") {
        retval = Model::instance()->getArtistList(titlePartialFilter(), genreFilter());
    } else if (what == "album") {
        retval = Model::instance()->getAlbumList(titlePartialFilter(), artistFilter(), genreFilter());
    } else {
        qFatal("No valid filter!");
    }

    return retval;
}

void MusicLib::rescan() {
    qDebug() << "emitting rescan" << libPath();

    if (!scannerThread_.isRunning()) { scannerThread_.start(); }

    Model::instance()->copyLibToTmp();
    Model::instance()->clearMusicLib();

    emit startScan(libPath());
}

void MusicLib::setGenreList() {
    genreList_ = getList("genre");
    genreList_.sort();
    genreList_.prepend(ALL_FILTER);
    emit genreListChanged();
}

void MusicLib::setArtistList() {
    artistList_ = getList("artist");
    artistList_.sort();
    artistList_.prepend(ALL_FILTER);
    emit artistListChanged();
}

void MusicLib::setAlbumList() {
    albumList_ = getList("album");
    albumList_.sort();
    albumList_.prepend(ALL_FILTER);
    emit albumListChanged();
}

void MusicLib::scanStarted() {
    setScanning(true);
}

void MusicLib::scanUpdate() {
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

void MusicLib::scanFinished() {
    // Setting the query to something invalid to have the check in setDisplayLib
    // do the right thing and display stuff as expected.
    // FIXME: Having to do that makes me feel dirty. Is the concept sound?
    lastDisplayLibQuery_ = "-1";
    Model::instance()->restoreMetaData();
    emit musicLibChanged();
    setScanning(false);
}

QString MusicLib::titlePartialFilter() const {
    return titlePartialFilter_;
}
void MusicLib::setTitlePartialFilter(const QString& titlePartialFilter) {
    titlePartialFilter_ = titlePartialFilter;
    emit titlePartialFilterChanged();
}

bool MusicLib::checkVal(const QString& check, const QString& val) const {
    return check == "" || check == val;
}
