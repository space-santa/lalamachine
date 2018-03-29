#include <QDebug>
#include <QMutexLocker>
#include <QRegularExpression>
#include <QSqlError>

#include "config.h"
#include "model.h"

const QMap<Model::SortWhat, QString> Model::SORT_MAP = initSortMap();

QMap<Model::SortWhat, QString> Model::initSortMap() {
    QMap<SortWhat, QString> tmp;
    tmp.insert(Model::ALBUM, "album");
    tmp.insert(Model::ARTIST, "artist");
    tmp.insert(Model::GENRE, "genre");
    tmp.insert(Model::TRACK, "track");
    tmp.insert(Model::TITLE, "title");
    tmp.insert(Model::COMMENT, "comment");
    tmp.insert(Model::LENGTH, "length");
    tmp.insert(Model::DATEADDED, "dateAdded");
    tmp.insert(Model::DISCNUMBER, "discNumber");

    return tmp;
}

Model::Model() {
    init();
}
Model::~Model() {
    db_.close();
}

void Model::init() {
    mutex_ = QSharedPointer<QMutex>(new QMutex());

    db_ = QSqlDatabase::addDatabase("QSQLITE");
    db_.setDatabaseName(Config::MUSICLIBDB);
    db_.open();
    updateTable();
}

QStringList Model::genre(const QString& filter) {
    QSqlQuery result = db_.exec(Model::genreQuery(filter));
    return Model::resultToList(result, "genre");
}

QString Model::getSortQueryString(const QString& title,
                                  const QString& genre,
                                  const QString& artist,
                                  const QString& album,
                                  const SortWhat& what,
                                  bool sortAsc) {
    QString query("SELECT * FROM musiclib WHERE title NOT NULL ");

    if (!title.isEmpty()) {
        query.append("AND UPPER(title) LIKE '%");
        query.append(escapeString(title.toUpper()));
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

        if (!album.isEmpty()) {
            query.append("AND album = '");
            query.append(escapeString(album));
            query.append("'");
        }
    }

    query.append(" ORDER BY ");
    query.append(SORT_MAP.value(what));
    query.append(" ");

    if (sortAsc) {
        query.append("ASC");
    } else {
        query.append("DESC");
    }

    query.append(", artist, album, discNumber, track");

    qDebug() << "ZZZ" << query;
    return query;
}

QString Model::genreQuery(const QString& filter) {
    QString query("SELECT DISTINCT genre FROM musiclib %1 ORDER BY genre ASC");

    if (!filter.isEmpty()) {
        query = query.arg("WHERE UPPER(genre) LIKE '%%1%'").arg(Model::escapeString(filter.toUpper()));
    } else {
        query = query.arg("");
    }

    return query.simplified();
}

QString Model::artistQuery(const QString& artist, const QString& genre) {
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

QString Model::albumQuery(const QString& album, const QString& artist, const QString& genre) {
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

QStringList Model::resultToList(QSqlQuery result, const QString& what) {
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

    if (!tmplist.contains("dateAdded")) {
        qDebug() << db_.exec("ALTER TABLE musiclib ADD COLUMN dateAdded TEXT").lastError();
    }

    if (!tmplist.contains("discNumber")) {
        qDebug() << db_.exec("ALTER TABLE musiclib ADD COLUMN discNumber INT NOT NULL "
                             "DEFAULT 1")
                        .lastError();
    }
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

void Model::ensureAllTables() {
    createLibTable("musiclib");
}

void Model::createLibTable(const QString& name) {
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
        qs.append("`dateAdded` TEXT,\n");
        qs.append("`discNumber` int NOT NULL DEFAULT 1\n");
        qs.append(")");

        QMutexLocker locker(mutex_.data());
        qDebug() << db_.exec(qs.arg(name)).lastError();
    }
}

void Model::copyLibToTmp() {
    createLibTable("tmplib");
    QString query("insert into tmplib SELECT * from musiclib");
    QMutexLocker locker(mutex_.data());
    qDebug() << db_.exec(query).lastError();
}

void Model::clearMusicLib() {
    QString query("DELETE FROM musiclib");
    QMutexLocker locker(mutex_.data());
    qDebug() << db_.exec(query).lastError();
}

void Model::restoreMetaData() {
    QStringList tables = db_.tables();
    if (!tables.contains("musiclib") || !tables.contains("tmplib")) {
        return;
    }

    QSqlQuery records = db_.exec("SELECT * FROM musiclib");

    db_.transaction();
    while (records.next()) {
        QString mrl = records.value("mrl").toString();
        QSqlQuery tmprec = db_.exec(QString("SELECT dateAdded FROM tmplib WHERE mrl='%1'").arg(escapeString(mrl)));

        tmprec.first();
        QString tmpdate = tmprec.value("dateAdded").toString();

        if (tmpdate.isEmpty()) {
            continue;
        }

        QString query("UPDATE musiclib SET dateAdded='%1' WHERE mrl='%2'");
        db_.exec(query.arg(tmpdate).arg(escapeString(mrl))).lastError();
    }
    db_.commit();
    qDebug() << db_.exec("DROP TABLE tmplib").lastError();
}

QPair<int, QJsonArray> Model::queryResultToJson(QSqlQuery result) {
    QJsonArray retval;

    int totalLength = 0;

    while (result.next()) {
        QJsonObject tmp;

        int len = result.value("length").toInt();
        totalLength += len;

        tmp.insert("album", result.value("album").toString());
        tmp.insert("artist", result.value("artist").toString());
        tmp.insert("genre", result.value("genre").toString());
        tmp.insert("comment", result.value("comment").toString());
        tmp.insert("track", result.value("track").toInt());
        tmp.insert("title", result.value("title").toString());
        tmp.insert("mrl", result.value("mrl").toString());
        tmp.insert("path", result.value("path").toString());
        tmp.insert("length", len);
        tmp.insert("lengthString", result.value("lengthString").toString());
        tmp.insert("year", result.value("year").toInt());
        tmp.insert("dateAdded", result.value("dateAdded").toString());
        tmp.insert("discNumber", result.value("discNumber").toInt());

        retval.append(tmp);
    }

    return QPair<int, QJsonArray>(totalLength, retval);
}

QPair<int, QJsonArray> Model::runSetDisplayQuery(const QString& query) {
    QMutexLocker locker(mutex_.data());
    return Model::queryResultToJson(db_.exec(query));
}

QJsonArray Model::getAlbumTracks(const QString& album) {
    QString query("SELECT * FROM musiclib WHERE album = '%1' ORDER BY track");

    QMutexLocker locker(mutex_.data());
    QSqlQuery result = db_.exec(query.arg(escapeString(album)));

    return Model::queryResultToJson(result).second;
}

QString Model::getDateAddedByMrl(const QString& mrl) const {
    QString query("SELECT dateAdded FROM musiclib WHERE mrl='%1' OR path='%1'");
    QSqlQuery result = db_.exec(query.arg(escapeString(mrl)));
    result.first();
    return result.value("dateAdded").toString();
}

QJsonObject Model::getMetadataForMrl(const QString& mrl) const {
    return getMetadataForMrl(QUrl::fromLocalFile(mrl));
}

QJsonObject Model::getMetadataForMrl(const QUrl& mrl) const {
    QString query("SELECT * FROM musiclib WHERE mrl='%1' OR path='%1'");
    query = query.arg(escapeString(cleanPath(mrl.toLocalFile())));
    qDebug() << query;
    QSqlQuery result = db_.exec(query);
    QJsonObject retval = Model::queryResultToJson(result).second.first().toObject();
    qDebug() << retval;
    return retval;
}

QStringList Model::getGenreList(const QString& filter) {
    QStringList retval;
    QSqlQuery result = db_.exec(genreQuery(filter));

    while (result.next()) {
        QString tmp = result.value("genre").toString();

        if (tmp != "") {
            retval << tmp;
        }
    }

    return retval;
}

QStringList Model::getArtistList(const QString& artist, const QString& genre) {
    QStringList retval;
    QSqlQuery result = db_.exec(artistQuery(artist, genre));

    while (result.next()) {
        QString tmp = result.value("artist").toString();

        if (tmp != "") {
            retval << tmp;
        }
    }

    return retval;
}

QStringList Model::getAlbumList(const QString& album, const QString& artist, const QString& genre) {
    QStringList retval;
    QSqlQuery result = db_.exec(albumQuery(album, artist, genre));

    while (result.next()) {
        QString tmp = result.value("album").toString();

        if (tmp != "") {
            retval << tmp;
        }
    }

    return retval;
}

QString Model::cleanPath(QString mrl) {
    return mrl.remove(QRegularExpression("^file://"));
}