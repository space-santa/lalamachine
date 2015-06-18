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

#include "musiclibscanner.h"

#include <QElapsedTimer>
#include <QDir>
#include <QDirIterator>
#include <QMutexLocker>
#include <QSqlError>
#include <QSqlQuery>
#include <QRegularExpression>
#include "metadataprovider.h"
#include "musiclib.h"
#include "tags.h"

MusicLibScanner::MusicLibScanner(QObject *parent) : QObject(parent) {}

void MusicLibScanner::setDb(QSqlDatabase *db) { scanDb_ = db; }

void MusicLibScanner::scanLib(const QString &path)
{
    QElapsedTimer timer;
    timer.start();

    if (path == "" || !QDir(path).exists()) {
        qCritical("I can't scan a non-existing folder.");
        return;
    }

    qDebug() << "Start scan";
    emit scanStarted();
    Tags lib;
    MetaDataProvider meta;
    QDir rootDir(path);

    if (!rootDir.exists()) {
        qDebug() << "Dir not found.";
    } else {
        QDirIterator it(rootDir, QDirIterator::Subdirectories);

        QElapsedTimer metaTimer;

        // We begin a transaction here.
        scanDb_->transaction();

        while (it.hasNext()) {
            QString line = it.next();

            if (suffixCheck(line)) {
                lib = meta.metaData(QUrl(line));
                metaTimer.restart();

                // This is to mitigate another problem. We assume that all files
                // with the correct sufix are good. Problem is that they might
                // not be.
                // For testing, my current lib has 4583 tracks, 1 without title
                // all have a length.
                if (not lib.isValid()) continue;

                // Adding all queries to the transaction.
                scanDb_->exec(getTrackQuery(lib) + ";\n");
                qDebug() << "Query added" << metaTimer.elapsed();
            }
        }
        QMutexLocker locker(mutex_.data());
        // Now commit everything at once. Last time I checked this took
        // 189893 ms (for comparison, the old approach takes ~698612ms (=*3.67))
        scanDb_->commit();
    }

    qDebug() << "End scan" << timer.elapsed();
    emit scanComplete();
}
QSharedPointer<QMutex> MusicLibScanner::mutex() const { return mutex_; }

void MusicLibScanner::setMutex(const QSharedPointer<QMutex> &mutex)
{
    mutex_ = mutex;
}

QString MusicLibScanner::getTrackQuery(Tags track)
{
    QString query("INSERT into `musiclib` ");
    query.append("(`album`, `artist`, `comment`, `genre`, `length`, ");
    query.append("`lengthString`, `mrl`, `path`, `title`, `track`, `year`) VALUES ");

    QString valuesA("('%1', '%2', '%3', '%4', '%5', '%6', ");
    QString valuesB("'%1', '%2', '%3', '%4', '%5')");
    query.append(valuesA.arg(MusicLib::escapeString(track.album_))
                     .arg(MusicLib::escapeString(track.artist_))
                     .arg(MusicLib::escapeString(track.comment_))
                     .arg(MusicLib::escapeString(track.genre_))
                     .arg(MusicLib::escapeString(track.length_))
                     .arg(MusicLib::escapeString(track.lengthString_)));
    query.append(valuesB.arg(MusicLib::escapeString(track.mrl_))
                     .arg(MusicLib::escapeString(track.path_))
                     .arg(MusicLib::escapeString(track.title_))
                     .arg(MusicLib::escapeString(track.track_))
                     .arg(MusicLib::escapeString(track.year_)));

    return query;
}

void MusicLibScanner::addTracksToDB(QString query)
{
    QString tmp("BEGIN;\n%1;\nCOMMIT;");

    QMutexLocker locker(mutex_.data());

    QElapsedTimer timer;
    timer.start();
    QSqlError err = scanDb_->exec(tmp.arg(query)).lastError();
    qDebug() << "dbase query took" << timer.elapsed();

    if (err.type() > 0) {
        qDebug() << "SQL error -----------\n"
                 << "SQL error while adding track\n"
                 << "SQL error " << err.text() << "\n"
                 << "SQL error ----------\n";
    }

    emit trackAdded();
}

bool MusicLibScanner::suffixCheck(const QString &val) const
{
    if (val.endsWith(".mp3")) {
        return true;
    }

    if (val.endsWith(".m4a")) {
        return true;
    }

    if (val.endsWith(".ogg")) {
        return true;
    }

    return false;
}
