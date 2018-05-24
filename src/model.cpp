#include <QDebug>
#include <QMutexLocker>
#include <QRegularExpression>

#include "QueryBuilder.h"
#include "config.h"
#include "exceptions.h"
#include "model.h"

Model::Model(std::unique_ptr<IMainDB> mainDB) : db_(std::move(mainDB)) {
    init();
}

void Model::init() {
    updateTable();
}

QStringList Model::genre(const QString& filter) {
    auto result = db_->exec(QueryBuilder::genreQuery(filter));
    return Model::resultToList(result, "genre");
}

QStringList Model::resultToList(const std::unique_ptr<IQueryResult>& result, const QString& what) {
    QStringList retval;

    while (result->next()) {
        QString tmp = result->value(what).toString();

        if (!tmp.isEmpty()) {
            retval << tmp;
        }
    }

    return retval;
}

void Model::updateTable() {
    QStringList tables = db_->tables();

    if (!tables.contains("musiclib")) {
        ensureAllTables();
        return;
    }

    QString query("PRAGMA table_info(musiclib)");
    auto record = db_->exec(query);

    QStringList tmplist;
    while (record->next()) {
        tmplist << record->value("name").toString();
    }

    if (!tmplist.contains("dateAdded")) {
        try {
            db_->exec("ALTER TABLE musiclib ADD COLUMN dateAdded TEXT");
        } catch (const QueryError& error) {
            qDebug() << error.what();
        }
    }

    if (!tmplist.contains("discNumber")) {
        try {
            db_->exec("ALTER TABLE musiclib ADD COLUMN discNumber INT NOT NULL DEFAULT 1");
        } catch (const QueryError& error) {
            qDebug() << error.what();
        }
    }
}

void Model::ensureAllTables() {
    createLibTable("musiclib");
}

void Model::createLibTable(const QString& name) {
    QStringList tables = db_->tables();

    if (!tables.contains(name)) {
        QString qs("CREATE TABLE `%1` ");
        qs.append("(\n");
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
        qs.append("`dateAdded` TEXT,\n");
        qs.append("`discNumber` int NOT NULL DEFAULT 1\n");
        qs.append(")");

        try {
            db_->exec(qs.arg(name));
        } catch (const QueryError& error) {
            qDebug() << error.what();
        }
    }
}

void Model::copyLibToTmp() {
    createLibTable("tmplib");
    QString query("insert into tmplib SELECT * from musiclib");

    try {
        db_->exec(query);
    } catch (const QueryError& error) {
        qDebug() << error.what();
    }
}

void Model::clearMusicLib() {
    QString query("DELETE FROM musiclib");

    try {
        db_->exec(query);
    } catch (const QueryError& error) {
        qDebug() << error.what();
    }
}

void Model::setDateAddedForMrl(const QString& dateAdded, const QString& mrl) {
    QString query("UPDATE musiclib SET dateAdded='%1' WHERE mrl='%2'");

    try {
        db_->exec(query.arg(dateAdded).arg(QueryBuilder::escapeString(mrl)));
    } catch (const QueryError& error) {
        qDebug() << error.what();
    }
}

QString Model::getDateAddedFromTmpLibForMrl(const QString& mrl) {
    auto tmprec =
        db_->exec(QString("SELECT dateAdded FROM tmplib WHERE mrl='%1'").arg(QueryBuilder::escapeString(mrl)));

    tmprec->first();
    return tmprec->value("dateAdded").toString();
}

void Model::checkIfTablesExist() const {
    auto tables = db_->tables();

    if (!tables.contains("musiclib")) {
        throw TableNotFoundError("musiclib");
    }
    if (!tables.contains("tmplib")) {
        throw TableNotFoundError("tmplib");
    }
}

void Model::restoreMetaData() {
    checkIfTablesExist();
    auto records = db_->exec("SELECT * FROM musiclib");

    db_->transaction();
    while (records->next()) {
        QString mrl = records->value("mrl").toString();
        QString tmpdate = getDateAddedFromTmpLibForMrl(mrl);

        if (tmpdate.isEmpty()) {
            continue;
        }

        setDateAddedForMrl(tmpdate, mrl);
    }
    db_->commit();

    try {
        db_->exec("DROP TABLE tmplib");
    } catch (const QueryError& error) {
        qDebug() << error.what();
    }
}

QPair<int, QJsonArray> Model::queryResultToJson(const std::unique_ptr<IQueryResult>& result) {
    QJsonArray retval;

    int totalLength = 0;

    while (result->next()) {
        QJsonObject tmp;

        int len = result->value("length").toInt();
        totalLength += len;

        tmp.insert("album", result->value("album").toString());
        tmp.insert("artist", result->value("artist").toString());
        tmp.insert("genre", result->value("genre").toString());
        tmp.insert("comment", result->value("comment").toString());
        tmp.insert("track", result->value("track").toInt());
        tmp.insert("title", result->value("title").toString());
        tmp.insert("mrl", result->value("mrl").toString());
        tmp.insert("path", result->value("path").toString());
        tmp.insert("length", len);
        tmp.insert("lengthString", result->value("lengthString").toString());
        tmp.insert("year", result->value("year").toInt());
        tmp.insert("dateAdded", result->value("dateAdded").toString());
        tmp.insert("discNumber", result->value("discNumber").toInt());

        retval.append(tmp);
    }

    return QPair<int, QJsonArray>(totalLength, retval);
}

QPair<int, QJsonArray> Model::runSetDisplayQuery(const QString& query) {
    return Model::queryResultToJson(db_->exec(query));
}

QJsonArray Model::getAlbumTracks(const QString& album) {
    QString query("SELECT * FROM musiclib WHERE album = '%1' ORDER BY track");

    auto result = db_->exec(query.arg(QueryBuilder::escapeString(album)));

    return Model::queryResultToJson(result).second;
}

QString Model::getDateAddedByMrl(const QString& mrl) const {
    QString query("SELECT dateAdded FROM musiclib WHERE mrl='%1' OR path='%1'");
    auto result = db_->exec(query.arg(QueryBuilder::escapeString(mrl)));
    result->first();
    return result->value("dateAdded").toString();
}

QJsonObject Model::getMetadataForMrl(const QString& mrl) const {
    return getMetadataForMrl(QUrl::fromLocalFile(mrl));
}

QJsonObject Model::getMetadataForMrl(const QUrl& mrl) const {
    QString query("SELECT * FROM musiclib WHERE mrl='%1' OR path='%1'");
    query = query.arg(QueryBuilder::escapeString(cleanPath(mrl.toLocalFile())));
    qDebug() << query;
    auto result = db_->exec(query);
    QJsonObject retval = Model::queryResultToJson(result).second.first().toObject();
    qDebug() << retval;
    return retval;
}

QStringList Model::getGenreList(const QString& filter) const {
    QStringList retval;
    auto result = db_->exec(QueryBuilder::genreQuery(filter));

    while (result->next()) {
        QString tmp = result->value("genre").toString();

        if (tmp != "") {
            retval << tmp;
        }
    }

    return retval;
}

QStringList Model::getArtistList(const QString& artist, const QString& genre) const {
    QStringList retval;
    auto result = db_->exec(QueryBuilder::artistQuery(artist, genre));

    while (result->next()) {
        QString tmp = result->value("artist").toString();

        if (tmp != "") {
            retval << tmp;
        }
    }

    return retval;
}

QStringList Model::getAlbumList(const QString& album, const QString& artist, const QString& genre) const {
    QStringList retval;
    auto result = db_->exec(QueryBuilder::albumQuery(album, artist, genre));

    while (result->next()) {
        QString tmp = result->value("album").toString();

        if (tmp != "") {
            retval << tmp;
        }
    }

    return retval;
}

QString Model::cleanPath(QString mrl) {
    return mrl.remove(QRegularExpression("^file://"));
}
