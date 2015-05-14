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
#include "metadataprovider.h"
#include "musiclib.h"

MusicLibScanner::MusicLibScanner(QObject *parent) : QObject(parent) {}

void MusicLibScanner::setDb(QSqlDatabase *db) { scanDb_ = db; }

void MusicLibScanner::scanLib(const QString &path)
{
    QElapsedTimer timer;
    timer.start();

    counterThreshold_ = 10;

    if (path == "" || !QDir(path).exists()) {
        qCritical("I can't scan a non-existing folder.");
        return;
    }

    qDebug() << "Start scan";
    emit scanStarted();
    QVector<QString> lib(11);
    MetaDataProvider meta;
    QDir rootDir(path);

    if (!rootDir.exists()) {
        qDebug() << "Dir not found.";
    } else {
        QDirIterator it(rootDir, QDirIterator::Subdirectories);

        QElapsedTimer metaTimer;
        while (it.hasNext()) {
            QString line = it.next();

            if (suffixCheck(line)) {
                metaTimer.restart();
                lib = meta.metaData(QUrl(line));
                qDebug() << "meta finished" << metaTimer.elapsed();
                metaTimer.restart();
                addTrackToDB(lib.at(0),
                             lib.at(1),
                             lib.at(2),
                             lib.at(3),
                             lib.at(4),
                             lib.at(5),
                             lib.at(6),
                             lib.at(7),
                             lib.at(8),
                             lib.at(9),
                             lib.at(10));
                qDebug() << "track added" << metaTimer.elapsed();
            }
        }
    }

    qDebug() << "End scan" << timer.elapsed();
    emit scanComplete();
}
QSharedPointer<QMutex> MusicLibScanner::mutex() const { return mutex_; }

void MusicLibScanner::setMutex(const QSharedPointer<QMutex> &mutex)
{
    mutex_ = mutex;
}

void MusicLibScanner::addTrackToDB(QString album,
                                   QString artist,
                                   QString comment,
                                   QString genre,
                                   QString length,
                                   QString lengthString,
                                   QString mrl,
                                   QString path,
                                   QString title,
                                   QString track,
                                   QString year)
{
    QString query("INSERT into `musiclib` ");
    query.append("(`album`, `artist`, `comment`, `genre`, `length`, ");
    query.append("`lengthString`, `mrl`, `path`, `title`, `track`, `year`) ");
    query.append("VALUES (");
    query.append(QString("'%1', '%2', '%3', ")
                     .arg(MusicLib::escapeString(album),
                          MusicLib::escapeString(artist),
                          MusicLib::escapeString(comment)));
    query.append(QString("'%1', %2, '%3', '%4', '%5', '%6', %7, %8)")
                     .arg(MusicLib::escapeString(genre),
                          length,
                          lengthString,
                          MusicLib::escapeString(mrl),
                          MusicLib::escapeString(path),
                          MusicLib::escapeString(title),
                          track,
                          year));

    QMutexLocker locker(mutex_.data());
    QSqlError err = scanDb_->exec(query).lastError();

    if (err.type() > 0) {
        //        qDebug() << "\n-----------\n"
        //                 << err.text() << "\n"
        //                 << mrl << "\n" << query << "\n----------\n";
    }

    setAddCounter();
    if (addCounter() == 0) {
        emit trackAdded();
    }
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
int MusicLibScanner::addCounter() const { return addCounter_; }

void MusicLibScanner::setAddCounter()
{
    if (addCounter_ < counterThreshold_) {
        ++addCounter_;
    } else {
        addCounter_ = 0;
        counterThreshold_ += 10;
    }
}
