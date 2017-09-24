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

#include <QDateTime>
#include <QDir>
#include <QDirIterator>
#include <QElapsedTimer>
#include <QMutexLocker>
#include <QRegularExpression>
#include <QSqlError>
#include <QSqlQuery>
#include "config.h"
#include "metadataprovider.h"
#include "model.h"
#include "musiclib.h"
#include "tags.h"

void MusicLibScanner::scanLib(const QString &path) {
  QSqlDatabase scanDb = QSqlDatabase::addDatabase("QSQLITE", "scanner");
  scanDb.setDatabaseName(Config::MUSICLIBDB);
  QElapsedTimer timer;
  timer.start();

  if (path == "" || !QDir(path).exists()) {
    qCritical("I can't scan a non-existing folder.");
    return;
  }

  ;
  if (!scanDb.open()) {
    qDebug() << "Can't open dbase..." << scanDb.lastError().type();
    return;
  }

  qDebug() << "Start scan";
  emit scanStarted();
  Tags tmp;
  MetaDataProvider meta;
  QDir rootDir(path);

  if (!rootDir.exists()) {
    qDebug() << "Dir not found.";
  } else {
    QDirIterator it(rootDir, QDirIterator::Subdirectories);

    QString date = QDateTime::currentDateTime().toString("yyyy-MM-dd");
    // We begin a transaction here.
    scanDb.transaction();
    QString line;
    QString error;

    while (it.hasNext()) {
      line = it.next();

      qDebug() << line;

      if (suffixCheck(line)) {
        tmp = meta.metaData(QUrl::fromLocalFile(line));

        // This is to mitigate another problem. We assume that all files
        // with the correct sufix are good. Problem is that they might
        // not be.
        if (!tmp.isValid()) {
          continue;
        }

        // Adding all queries to the transaction.
        error = scanDb.exec(getTrackQuery(tmp, date) + ";\n")
                    .lastError()
                    .text()
                    .trimmed();

        if (!error.isEmpty()) {
          qDebug() << error;
        }
      }
    }
    qDebug() << "pre commit" << timer.elapsed();
    timer.restart();
    // Now commit everything at once.
    scanDb.commit();
    qDebug() << "post commit" << timer.elapsed();
    timer.restart();
  }

  scanDb.close();
  qDebug() << "End scan" << timer.elapsed();
  emit scanComplete();
}

QString MusicLibScanner::getTrackQuery(Tags track, const QString date) {
  QString query(
      "INSERT into `musiclib` "
      "(`album`, `artist`, `comment`, `genre`, `length`, "
      "`lengthString`, `mrl`, `path`, `title`, `track`, `"
      "year`, `dateAdded`) "
      "VALUES ");

  QString valuesA("('%1', '%2', '%3', '%4', '%5', '%6', ");
  QString valuesB("'%1', '%2', '%3', '%4', '%5', '%6')");
  query.append(valuesA.arg(Model::escapeString(track.album_))
                   .arg(Model::escapeString(track.artist_))
                   .arg(Model::escapeString(track.comment_))
                   .arg(Model::escapeString(track.genre_))
                   .arg(Model::escapeString(track.length_))
                   .arg(Model::escapeString(track.lengthString_)));
  query.append(valuesB.arg(Model::escapeString(track.mrl_))
                   .arg(Model::escapeString(track.path_))
                   .arg(Model::escapeString(track.title_))
                   .arg(Model::escapeString(track.track_))
                   .arg(Model::escapeString(track.year_))
                   .arg(Model::escapeString(date)));

  qDebug() << query;
  return query;
}

bool MusicLibScanner::suffixCheck(const QString &val) {
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
