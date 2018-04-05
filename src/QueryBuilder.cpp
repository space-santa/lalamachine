#include "QueryBuilder.h"

const QMap<QueryBuilder::SortWhat, QString> QueryBuilder::SORT_MAP = initSortMap();

QMap<QueryBuilder::SortWhat, QString> QueryBuilder::initSortMap() {
    QMap<QueryBuilder::SortWhat, QString> tmp;
    tmp.insert(QueryBuilder::ALBUM, "album");
    tmp.insert(QueryBuilder::ARTIST, "artist");
    tmp.insert(QueryBuilder::GENRE, "genre");
    tmp.insert(QueryBuilder::TRACK, "track");
    tmp.insert(QueryBuilder::TITLE, "title");
    tmp.insert(QueryBuilder::COMMENT, "comment");
    tmp.insert(QueryBuilder::LENGTH, "length");
    tmp.insert(QueryBuilder::DATEADDED, "dateAdded");
    tmp.insert(QueryBuilder::DISCNUMBER, "discNumber");

    return tmp;
}

QString QueryBuilder::escapeString(QString string) {
    return string.replace("\'", "\'\'");
}

QString QueryBuilder::albumQuery(const QString& album, const QString& artist, const QString& genre) {
    QString query("SELECT DISTINCT album FROM musiclib WHERE album NOT NULL ");

    if (!album.isEmpty()) {
        query.append("AND UPPER(album) LIKE '%");
        query.append(QueryBuilder::escapeString(album.toUpper()));
        query.append("%' ");
    } else {
        if (!genre.isEmpty()) {
            query.append("AND genre = '");
            query.append(QueryBuilder::escapeString(genre));
            query.append("' ");
        }

        if (!artist.isEmpty()) {
            query.append("AND artist = '");
            query.append(QueryBuilder::escapeString(artist));
            query.append("'");
        }
    }

    query.append(" ORDER BY album ASC");

    return query.simplified();
}

QString QueryBuilder::artistQuery(const QString& artist, const QString& genre) {
    QString query("SELECT DISTINCT artist FROM musiclib WHERE artist NOT NULL ");

    if (!artist.isEmpty()) {
        query.append("AND UPPER(artist) LIKE '%");
        query.append(QueryBuilder::escapeString(artist.toUpper()));
        query.append("%' ");
    } else {
        if (!genre.isEmpty()) {
            query.append("AND genre = '");
            query.append(QueryBuilder::escapeString(genre));
            query.append("' ");
        }
    }

    query.append(" ORDER BY artist ASC");

    return query.simplified();
}

QString QueryBuilder::genreQuery(const QString& filter) {
    QString query("SELECT DISTINCT genre FROM musiclib %1 ORDER BY genre ASC");

    if (!filter.isEmpty()) {
        query = query.arg("WHERE UPPER(genre) LIKE '%%1%'").arg(QueryBuilder::escapeString(filter.toUpper()));
    } else {
        query = query.arg("");
    }

    return query.simplified();
}

QString QueryBuilder::getSortQueryString(const QString& title,
                                         const QString& genre,
                                         const QString& artist,
                                         const QString& album,
                                         const QueryBuilder::SortWhat& what,
                                         bool sortAsc) {
    QString query("SELECT * FROM musiclib WHERE title NOT NULL ");

    if (!title.isEmpty()) {
        query.append("AND UPPER(title) LIKE '%");
        query.append(QueryBuilder::escapeString(title.toUpper()));
        query.append("%' ");
    } else {
        if (!genre.isEmpty()) {
            query.append("AND genre = '");
            query.append(QueryBuilder::escapeString(genre));
            query.append("' ");
        }

        if (!artist.isEmpty()) {
            query.append("AND artist = '");
            query.append(QueryBuilder::escapeString(artist));
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
    return query;
}
