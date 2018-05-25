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

#include <QElapsedTimer>
#include <QFuture>
#include <QFutureWatcher>
#include <QJsonObject>
#include <QPair>
#include <QSqlQuery>
#include <QUrl>
#include <QtConcurrent>
#include <memory>

#include "DirWalker.h"
#include "MainDB.h"
#include "ScannerDB.h"
#include "autoplaylistmanager.h"
#include "config.h"
#include "metadataprovider.h"
#include "model.h"
#include "musiclibscanner.h"

MusicLib::MusicLib(QObject* parent) : QObject(parent), model(std::unique_ptr<IMainDB>(new MainDB())) {
    init();

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
    connect(&scannerWatcher, &QFutureWatcher<QSqlQuery>::finished, this, &MusicLib::scanFinished);

    setGenreList();
}

void MusicLib::init() {
    scanner_ = std::shared_ptr<MusicLibScanner>(
        new MusicLibScanner(std::unique_ptr<IScannerDB>(new ScannerDB()),
                            std::unique_ptr<IDirWalker>(new DirWalker()),
                            std::unique_ptr<IMetaDataProvider>(new MetaDataProvider())));

    sortAsc_ = true;
    scanning_ = false;
    what_ = QueryBuilder::ARTIST;
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

void MusicLib::setDisplayLib() {
    QElapsedTimer elapsed;
    elapsed.start();

    QString query = QueryBuilder::getSortQueryString(titlePartialFilter(),
                                                     genreFilter(),
                                                     artistFilter(),
                                                     albumFilter(),
                                                     what(),
                                                     sortAsc());

    if (query == lastDisplayLibQuery_) {
        qDebug() << "query didn't change, nothing to do.";
        return;
    }

    lastDisplayLibQuery_ = query;
    QPair<int, QJsonArray> tmp = model.runSetDisplayQuery(query);

    qDebug() << "runSetDisplayQuery took:" << elapsed.elapsed();

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

    if (!appStart_) {
        rescan();
    }

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
    if (sortAsc_ == val) {
        return;
    }

    // Only do things when there was an actual change.
    // We only want to setDisplayLib if necessary.
    sortAsc_ = val;
    emit sortAscChanged();
    setDisplayLib();
}

QueryBuilder::SortWhat MusicLib::what() const {
    return what_;
}

void MusicLib::setWhat(QueryBuilder::SortWhat val) {
    if (what_ == val) {
        return;
    }
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
    what_ = QueryBuilder::ARTIST;

    genreFilter_ = "";
    artistFilter_ = "";
    albumFilter_ = "";
    setTitlePartialFilter("");
}

QStringList MusicLib::getList(const QString& what) const {
    QStringList retval;

    if (what == "genre") {
        retval = model.getGenreList(titlePartialFilter());
    } else if (what == "artist") {
        retval = model.getArtistList(titlePartialFilter(), genreFilter());
    } else if (what == "album") {
        retval = model.getAlbumList(titlePartialFilter(), artistFilter(), genreFilter());
    } else {
        qFatal("No valid filter!");
    }

    return retval;
}

QJsonArray MusicLib::getAlbumTracks(const QString& album) {
    return model.getAlbumTracks(album);
}

QString MusicLib::getDateAddedByMrl(const QString& mrl) const {
    return model.getDateAddedByMrl(mrl);
}

QJsonObject MusicLib::getMetadataForMrl(const QString& mrl) const {
    return model.getMetadataForMrl(mrl);
}

QJsonObject MusicLib::getMetadataForMrl(const QUrl& mrl) const {
    return model.getMetadataForMrl(mrl);
}

void MusicLib::rescan() {
    if (scanning()) {
        qDebug() << "Scan is already in progress.";
        return;
    }

    qDebug() << "scanning" << libPath();

    model.copyLibToTmp();
    model.clearMusicLib();

    scanStarted();

    auto future = QtConcurrent::run(MusicLibScanner::scan, scanner_, libPath());
    scannerWatcher.setFuture(future);
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

void MusicLib::scanFinished() {
    QString somethingInvalidToHaveTheCheckInSetDisplayLibDoTheRightThing = "-1";
    lastDisplayLibQuery_ = somethingInvalidToHaveTheCheckInSetDisplayLibDoTheRightThing;
    model.restoreMetaData();
    emit musicLibChanged();
    setScanning(false);
}

QString MusicLib::titlePartialFilter() const {
    return titlePartialFilter_;
}
void MusicLib::setTitlePartialFilter(const QString& titlePartialFilter) {
    if (titlePartialFilter.length() < 3 && titlePartialFilter.length() > 0) {
        return;
    }
    titlePartialFilter_ = titlePartialFilter;
    emit titlePartialFilterChanged();
}

bool MusicLib::checkVal(const QString& check, const QString& val) const {
    return check == "" || check == val;
}
