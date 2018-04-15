#include "ScannerDB.h"
#include <QDate>
#include <QDebug>
#include <QSqlError>
#include <QSqlQuery>
#include "QueryBuilder.h"
#include "config.h"
#include "exceptions.h"

ScannerDB::ScannerDB() : databaseName("scanner") {
}

void ScannerDB::open() {
    qDebug() << "this does nothing";
}

void ScannerDB::transaction() {
    queryList.clear();
}

void ScannerDB::addQuery(const Tags& tags) {
    QString date = QDateTime::currentDateTime().toString("yyyy-MM-dd");
    QString query = getTrackQuery(tags, date) + ";\n";
    queryList << query;
}

void ScannerDB::commit() {
    auto db = QSqlDatabase::addDatabase("QSQLITE", databaseName);
    db.setDatabaseName(Config::MUSICLIBDB);

    if (!db.open()) {
        throw OpenDatabaseError(db.lastError().text().toStdString());
    }

    db.transaction();

    for (const auto& query : queryList) {
        QString error = db.exec(query).lastError().text().trimmed();

        if (!error.isEmpty()) {
            throw AddQueryError(error.toStdString());
        }
    }

    qDebug() << "+++ commit +++";
    qDebug() << db.commit();
    qDebug() << db.lastError();
    qDebug() << "+++ end commit +++";
}

void ScannerDB::close() {
    auto db = QSqlDatabase::database(databaseName);
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
