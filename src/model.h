#pragma once

#include <QJsonArray>
#include <QJsonObject>
#include <QMutex>
#include <QSharedPointer>
#include <QSqlDatabase>
#include <QSqlQuery>
#include <QSqlResult>
#include <QString>
#include <QStringList>

class Model {
    friend class ModelTest;

public:
    Model();
    ~Model();

    enum SortWhat { TRACK, TITLE, COMMENT, LENGTH, GENRE, ARTIST, ALBUM, DATEADDED, DISCNUMBER };
    Q_ENUMS(SortWhat)

    enum SortHow { ASCENDING, DESCENDING };
    Q_ENUMS(SortHow)

    static const QMap<SortWhat, QString> SORT_MAP;
    static QMap<SortWhat, QString> initSortMap();

    QStringList genre(const QString& filter);
    QStringList artist(const QString& filter, const QString& genre = QString());
    QStringList album(const QString& filter, const QString& genre = QString(), const QString& artist = QString());

    QJsonObject trackDetails(const QString& mrl) const;

    static QString escapeString(QString string);
    static QString getSortQueryString(const QString& title,
                                      const QString& genre,
                                      const QString& artist,
                                      const QString& album,
                                      const SortWhat& what,
                                      bool sortAsc);
    static QString genreQuery(const QString& filter = QString());
    static QString artistQuery(const QString& artist = QString(), const QString& genre = QString());
    static QString albumQuery(const QString& album = QString(),
                              const QString& artist = QString(),
                              const QString& genre = QString());
    static QPair<int, QJsonArray> queryResultToJson(QSqlQuery result);
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
    QSqlDatabase db_;
    QSharedPointer<QMutex> mutex_;

    void init();
    static QStringList resultToList(QSqlQuery result, const QString& what);
    void updateTable();
    void newUpdateTable();
};
