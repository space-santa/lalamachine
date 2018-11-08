#pragma once

#include <QJsonArray>
#include <QJsonObject>
#include <QSqlQuery>
#include <QSqlResult>
#include <QString>
#include <QStringList>
#include <memory>

#include "IMainDB.h"
#include "QueryBuilder.h"

class Model {
    friend class ModelTest;

public:
    Model(std::unique_ptr<IMainDB> mainDB);

    Q_ENUMS(QueryBuilder::SortWhat)
    Q_ENUMS(QueryBuilder::SortHow)

    static QPair<int, QJsonArray> queryResultToJson(const std::unique_ptr<IQueryResult>& result);
    static QString cleanPath(QString mrl);

    QStringList getGenreList(const QString& filter = QString()) const;
    QStringList getArtistList(const QString& artist = QString(), const QString& genre = QString()) const;
    QStringList getAlbumList(const QString& album = QString(),
                             const QString& artist = QString(),
                             const QString& genre = QString()) const;

    QPair<int, QJsonArray> runSetDisplayQuery(const QString& query);
    QJsonArray getAlbumTracks(const QString& album);
    QJsonObject getMetadataForMrl(const QString& mrl) const;
    QJsonObject getMetadataForMrl(const QUrl& mrl) const;

private:
    std::unique_ptr<IMainDB> db_;
    static QStringList resultToList(const std::unique_ptr<IQueryResult>& result, const QString& what);
};
