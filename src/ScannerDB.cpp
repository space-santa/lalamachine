#include <QDate>
#include <QDebug>
#include <QSqlDatabase>
#include <QSqlError>
#include <QSqlQuery>
#include "QueryBuilder.h"
#include "ScannerDB.h"
#include "config.h"
#include "exceptions.h"

void ScannerDB::init() {
    auto db = QSqlDatabase::addDatabase("QSQLITE", Config::SCANNERDBNAME);
    db.setDatabaseName(Config::MUSICLIBDB);
    db.open();
}

void ScannerDB::kill() {
    QSqlDatabase::removeDatabase(Config::SCANNERDBNAME);
}

void ScannerDB::open() {
    qDebug() << "ScannerDB::open => this does nothing";
}

void ScannerDB::transaction() {
    queryList.clear();
}

void ScannerDB::addQuery(const QJsonObject& tags) {
    QString date = QDateTime::currentDateTime().toString("yyyy-MM-dd");
    QString query = getTrackQuery(tags) + ";\n";
    queryList << query;
}

void ScannerDB::clearMusicLib() {
    QString query("DELETE FROM musiclib");
    auto db = QSqlDatabase::database(Config::SCANNERDBNAME);

    try {
        db.exec(query);
    } catch (const QueryError& error) {
        qDebug() << error.what();
    }
}


void ScannerDB::commit() {
    init();
    clearMusicLib();
    {
        auto db = QSqlDatabase::database(Config::SCANNERDBNAME);
        db.transaction();

        for (const auto& query : queryList) {
            QString error = db.exec(query).lastError().text().trimmed();

            if (!error.isEmpty()) {
                qDebug() << "db.exec has an error:" << error;
            }
        }

        auto commitSuccessful = db.commit();

        if (!commitSuccessful) {
            kill();
            throw DatabaseCommitError(db.lastError().text().toStdString());
        }
    }
    kill();
}

void ScannerDB::close() {
    qDebug() << "ScannerDB::close => This does nothing.";
}

QString ScannerDB::getTrackQuery(const QJsonObject& track) {
    QString query("INSERT into `musiclib` "
                  "(`album`, `artist`, `comment`, `genre`, `length`, "
                  "`lengthString`, `mrl`, `path`, `title`, `track`, `"
                  "year`, `discNumber`) "
                  "VALUES ");

    QString valuesA("('%1', '%2', '%3', '%4', '%5', '%6', ");
    QString valuesB("'%1', '%2', '%3', '%4', '%5', '%6')");
    query.append(valuesA.arg(QueryBuilder::escapeString(track.value("album").toString()))
                     .arg(QueryBuilder::escapeString(track.value("artist").toString()))
                     .arg(QueryBuilder::escapeString(track.value("comment").toString()))
                     .arg(QueryBuilder::escapeString(track.value("genre").toString()))
                     .arg(QueryBuilder::escapeString(QString::number(track.value("length").toInt())))
                     .arg(QueryBuilder::escapeString(track.value("lengthString").toString())));
    query.append(valuesB.arg(QueryBuilder::escapeString(track.value("path").toString()))
                     .arg(QueryBuilder::escapeString(track.value("path").toString()))
                     .arg(QueryBuilder::escapeString(track.value("title").toString()))
                     .arg(QueryBuilder::escapeString(QString::number(track.value("track").toInt())))
                     .arg(QueryBuilder::escapeString(QString::number(track.value("year").toInt())))
                     .arg(QueryBuilder::escapeString(QString::number(track.value("discNumber").toInt()))));
    return query;
}
