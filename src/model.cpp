#include "model.h"

#include <QDebug>
#include <QMutexLocker>
#include <QSqlError>

#include "config.h"

Model::Model() {}

void Model::init() {
  db_ = QSqlDatabase::addDatabase("QSQLITE");
  db_.setDatabaseName(Config::MUSICLIBDB);
  db_.open();
}

QStringList Model::genre(const QString &filter) {
  QSqlQuery result = db_.exec(Model::genreQuery(filter));
  return Model::resultToList(result, "genre");
}

QString Model::genreQuery(const QString &filter) {
  QString query("SELECT DISTINCT genre FROM musiclib %1 ORDER BY genre ASC");

  if (!filter.isEmpty()) {
    query = query.arg("WHERE UPPER(genre) LIKE '%%1%'")
                .arg(Model::escapeString(filter.toUpper()));
  } else {
    query = query.arg("");
  }

  return query.simplified();
}

QString Model::artistQuery(const QString &artist, const QString &genre) {
  QString query("SELECT DISTINCT artist FROM musiclib WHERE artist NOT NULL ");

  if (!artist.isEmpty()) {
    query.append("AND UPPER(artist) LIKE '%");
    query.append(escapeString(artist.toUpper()));
    query.append("%' ");
  } else {
    if (!genre.isEmpty()) {
      query.append("AND genre = '");
      query.append(escapeString(genre));
      query.append("' ");
    }
  }

  query.append(" ORDER BY artist ASC");

  return query.simplified();
}

QString Model::albumQuery(const QString &album, const QString &artist,
                          const QString &genre) {
  QString query("SELECT DISTINCT album FROM musiclib WHERE album NOT NULL ");

  if (!album.isEmpty()) {
    query.append("AND UPPER(album) LIKE '%");
    query.append(escapeString(album.toUpper()));
    query.append("%' ");
  } else {
    if (!genre.isEmpty()) {
      query.append("AND genre = '");
      query.append(escapeString(genre));
      query.append("' ");
    }

    if (!artist.isEmpty()) {
      query.append("AND artist = '");
      query.append(escapeString(artist));
      query.append("'");
    }
  }

  query.append(" ORDER BY album ASC");

  return query.simplified();
}

QString Model::escapeString(QString string) {
  // return str.replace("\'", "\'\'").replace(",", "\'+\',\'+\'");
  return string.replace("\'", "\'\'");
}

QStringList Model::resultToList(QSqlQuery result, const QString &what) {
  QStringList retval;

  while (result.next()) {
    QString tmp = result.value(what).toString();

    if (!tmp.isEmpty()) {
      retval << tmp;
    }
  }

  return retval;
}

void Model::updateTable() {
  QStringList tables = db_.tables();

  if (!tables.contains("musiclib")) {
    ensureAllTables();
    return;
  }

  QString query("PRAGMA table_info(musiclib)");
  QMutexLocker locker(mutex_.data());
  QSqlQuery record = db_.exec(query);

  QStringList tmplist;
  while (record.next()) {
    tmplist << record.value("name").toString();
  }

  if (tmplist.contains("dateAdded")) {
    return;
  }

  qDebug()
      << db_.exec("ALTER TABLE musiclib ADD COLUMN dateAdded TEXT").lastError();
}

void Model::newUpdateTable() {
  // create tables
  // genre -- genreid, name
  // artist -- artistid, name, genreid (otm)
  // album -- albumid, genreid (otm), artistid (otm, various artists or null),
  //          name, disknumber?
  // track -- trackid, artistid (otm), genreid (otm), title, tracknumber,
  //          disknumber?, year

  bool oldstuff = false;
  if (oldstuff) {
    // rescan musiclib
    // We can't migrate the data because we now consider more fields
    // like disk number and I don't wantt to have an incomplete dbase.
  }
}

void Model::ensureAllTables() { createLibTable("musiclib"); }

void Model::createLibTable(const QString &name) {
  QStringList tables = db_.tables();

  if (!tables.contains(name)) {
    QString qs("CREATE TABLE `%1` ");
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
    qs.append("`year` int,\n");
    qs.append("`dateAdded` TEXT\n");
    qs.append(")");

    QMutexLocker locker(mutex_.data());
    qDebug() << db_.exec(qs.arg(name)).lastError();
  }
}