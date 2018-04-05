#pragma once

#include <QMap>
#include <QString>

class QueryBuilder {
public:
    enum SortWhat { TRACK, TITLE, COMMENT, LENGTH, GENRE, ARTIST, ALBUM, DATEADDED, DISCNUMBER };
    enum SortHow { ASCENDING, DESCENDING };

    static const QMap<QueryBuilder::SortWhat, QString> SORT_MAP;
    static QMap<QueryBuilder::SortWhat, QString> initSortMap();

    static QString albumQuery(const QString& album = QString(),
                              const QString& artist = QString(),
                              const QString& genre = QString());
    static QString artistQuery(const QString& artist = QString(), const QString& genre = QString());
    static QString escapeString(QString string);
    static QString genreQuery(const QString& filter = QString());
    static QString getSortQueryString(const QString& title,
                                      const QString& genre,
                                      const QString& artist,
                                      const QString& album,
                                      const SortWhat& what,
                                      bool sortAsc);
};