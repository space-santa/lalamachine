#include "ScannerDB.h"
#include <QDate>
#include <QSqlError>
#include <QSqlQuery>
#include "QueryBuilder.h"
#include "config.h"
#include "exceptions.h"

ScannerDB::ScannerDB() : db(QSqlDatabase::addDatabase("QSQLITE", "scanner")) {
    db.setDatabaseName(Config::MUSICLIBDB);
}

void ScannerDB::open() {
    if (!db.open()) {
        throw OpenDatabaseError(db.lastError().text().toStdString());
    }
}

void ScannerDB::transaction() {
    db.transaction();
}

void ScannerDB::addQuery(const Tags& tags) {
    QString date = QDateTime::currentDateTime().toString("yyyy-MM-dd");
    QString error = db.exec(getTrackQuery(tags, date) + ";\n").lastError().text().trimmed();

    if (!error.isEmpty()) {
        throw AddQueryError(error.toStdString());
    }
}

void ScannerDB::commit() {
    db.commit();
}

void ScannerDB::close() {
    db.close();
}

QString ScannerDB::getTrackQuery(const Tags& track, const QString& date) {
    QString query("INSERT into `musiclib` "
                  "(`album`, `artist`, `comment`, `genre`, `length`, "
                  "`lengthString`, `mrl`, `path`, `title`, `track`, `"
                  "year`, `dateAdded`, `discNumber`) "
                  "VALUES ");

    QString valuesA("('%1', '%2', '%3', '%4', '%5', '%6', ");
    QString valuesB("'%1', '%2', '%3', '%4', '%5', '%6', '%7')");
    query.append(valuesA.arg(QueryBuilder::escapeString(track.album_))
                     .arg(QueryBuilder::escapeString(track.artist_))
                     .arg(QueryBuilder::escapeString(track.comment_))
                     .arg(QueryBuilder::escapeString(track.genre_))
                     .arg(QueryBuilder::escapeString(track.length_))
                     .arg(QueryBuilder::escapeString(track.lengthString_)));
    query.append(valuesB.arg(QueryBuilder::escapeString(track.mrl_))
                     .arg(QueryBuilder::escapeString(track.path_))
                     .arg(QueryBuilder::escapeString(track.title_))
                     .arg(QueryBuilder::escapeString(track.track_))
                     .arg(QueryBuilder::escapeString(track.year_))
                     .arg(QueryBuilder::escapeString(date))
                     .arg(QueryBuilder::escapeString(track.disc_)));

    return query;
}