#include "ScannerDB.h"
#include <QDate>
#include <QSqlError>
#include "config.h"
#include "exceptions.h"
#include "model.h"

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
                     .arg(Model::escapeString(date))
                     .arg(Model::escapeString(track.disc_)));

    return query;
}