#include <QDate>
#include <QDebug>
#include <QSqlDatabase>
#include <QSqlError>
#include <QSqlQuery>
#include "QueryBuilder.h"
#include "ScannerDB.h"
#include "config.h"
#include "exceptions.h"

void ScannerDB::open() {
    qDebug() << "ScannerDB::open => this does nothing";
}

void ScannerDB::transaction() {
    queryList.clear();
}

void ScannerDB::addQuery(const QJsonObject& tags) {
    QString date = QDateTime::currentDateTime().toString("yyyy-MM-dd");
    QString query = getTrackQuery(tags, date) + ";\n";
    queryList << query;
}

void ScannerDB::commit() {
    auto db = QSqlDatabase::database(Config::SCANNERDBNAME);
    db.transaction();

    for (const auto& query : queryList) {
        QString error = db.exec(query).lastError().text().trimmed();

        if (!error.isEmpty()) {
            qDebug() << error;
        }
    }

    auto commitSuccessful = db.commit();

    if (!commitSuccessful) {
        throw DatabaseCommitError(db.lastError().text().toStdString());
    }
}

void ScannerDB::close() {
    qDebug() << "ScannerDB::close => This does nothing.";
}

QString ScannerDB::getTrackQuery(const QJsonObject& track, const QString& date) {
    QString query("INSERT into `musiclib` "
                  "(`album`, `artist`, `comment`, `genre`, `length`, "
                  "`lengthString`, `mrl`, `path`, `title`, `track`, `"
                  "year`, `dateAdded`, `discNumber`) "
                  "VALUES ");

    QString valuesA("('%1', '%2', '%3', '%4', '%5', '%6', ");
    QString valuesB("'%1', '%2', '%3', '%4', '%5', '%6', '%7')");
    query.append(valuesA.arg(QueryBuilder::escapeString(track.value("album").toString()))
                     .arg(QueryBuilder::escapeString(track.value("artist").toString()))
                     .arg(QueryBuilder::escapeString(track.value("comment").toString()))
                     .arg(QueryBuilder::escapeString(track.value("genre").toString()))
                     .arg(QueryBuilder::escapeString(track.value("length").toString()))
                     .arg(QueryBuilder::escapeString(track.value("lengthString").toString())));
    query.append(valuesB.arg(QueryBuilder::escapeString(track.value("path").toString()))
                     .arg(QueryBuilder::escapeString(track.value("path").toString()))
                     .arg(QueryBuilder::escapeString(track.value("title").toString()))
                     .arg(QueryBuilder::escapeString(track.value("track").toString()))
                     .arg(QueryBuilder::escapeString(track.value("year").toString()))
                     .arg(QueryBuilder::escapeString(date))
                     .arg(QueryBuilder::escapeString(track.value("disc").toString())));

    return query;
}
