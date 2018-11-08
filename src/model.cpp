#include <QDebug>
#include <QRegularExpression>

#include "QueryBuilder.h"
#include "QueryResult.h"
#include "config.h"
#include "exceptions.h"
#include "model.h"

Model::Model(std::unique_ptr<IMainDB> mainDB) : db_(std::move(mainDB)) {
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
        tmp.insert("discNumber", result->value("discNumber").toInt());

        retval.append(tmp);
    }

    return QPair<int, QJsonArray>(totalLength, retval);
}

QPair<int, QJsonArray> Model::runSetDisplayQuery(const QString& query) {
    auto result = db_->exec(query);
    return Model::queryResultToJson(result);
}

QJsonArray Model::getAlbumTracks(const QString& album) {
    QString query("SELECT * FROM musiclib WHERE album = '%1' ORDER BY track");

    auto result = db_->exec(query.arg(QueryBuilder::escapeString(album)));

    return Model::queryResultToJson(result).second;
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
