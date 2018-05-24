#pragma once

#include <QJsonArray>
#include <QJsonObject>
#include <QSqlQuery>
#include <QSqlResult>
#include <QString>
#include <QStringList>
#include <memory>
#include <mutex>

#include "IMainDB.h"
#include "QueryBuilder.h"

class Model {
    friend class ModelTest;

public:
    Model(std::unique_ptr<IMainDB> mainDB);

    Q_ENUMS(QueryBuilder::SortWhat)
    Q_ENUMS(QueryBuilder::SortHow)

    QStringList genre(const QString& filter);
    QStringList artist(const QString& filter, const QString& genre = QString());
    QStringList album(const QString& filter, const QString& genre = QString(), const QString& artist = QString());

    QJsonObject trackDetails(const QString& mrl) const;

    static QPair<int, QJsonArray> queryResultToJson(const std::unique_ptr<IQueryResult>& result);
    static QString cleanPath(QString mrl);

    void ensureAllTables();
    void createLibTable(const QString& name);
    void copyLibToTmp();
    void clearMusicLib();
    void restoreMetaData();
    QStringList getGenreList(const QString& filter = QString()) const;
    QStringList getArtistList(const QString& artist = QString(), const QString& genre = QString()) const;
    QStringList getAlbumList(const QString& album = QString(),
                             const QString& artist = QString(),
                             const QString& genre = QString()) const;

    QStringList getList(const QString& what) const;

    QPair<int, QJsonArray> runSetDisplayQuery(const QString& query);
    QJsonArray getAlbumTracks(const QString& album);
    QString getDateAddedByMrl(const QString& mrl) const;
    QJsonObject getMetadataForMrl(const QString& mrl) const;
    QJsonObject getMetadataForMrl(const QUrl& mrl) const;

private:
    std::unique_ptr<IMainDB> db_;
    std::mutex mutex_;

    void init();
    static QStringList resultToList(const std::unique_ptr<IQueryResult>& result, const QString& what);
    void updateTable();
    void setDateAddedForMrl(const QString& dateAdded, const QString& mrl);
    QString getDateAddedFromTmpLibForMrl(const QString& mrl);
    void checkIfTablesExist() const;
};
