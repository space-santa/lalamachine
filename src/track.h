#pragma once

#include <QJsonObject>
#include <QString>

class Track {
public:
    Track() {
    }
    Track(const QJsonObject& json) {
        fromJson(json);
    }

    void fromJson(const QJsonObject& json) {
        album_ = json.value("album").toString();
        artist_ = json.value("artist").toString();
        genre_ = json.value("genre").toString();
        comment_ = json.value("comment").toString();
        track_ = json.value("track").toInt();
        title_ = json.value("title").toString();
        mrl_ = json.value("mrl").toString();
        path_ = json.value("path").toString();
        length_ = json.value("length").toInt();
        lengthString_ = json.value("lengthString").toString();
        year_ = json.value("year").toInt();
        disc_ = json.value("discNumber").toInt();
        id_ = json.value("id").toInt();
    }
    QJsonObject toJson() const {
        QJsonObject tmp;

        tmp.insert("album", album_);
        tmp.insert("artist", artist_);
        tmp.insert("genre", genre_);
        tmp.insert("comment", comment_);
        tmp.insert("track", track_);
        tmp.insert("title", title_);
        tmp.insert("mrl", mrl_);
        tmp.insert("path", path_);
        tmp.insert("length", length_);
        tmp.insert("lengthString", lengthString_);
        tmp.insert("year", year_);
        tmp.insert("discNumber", disc_);
        tmp.insert("id", id_);

        return tmp;
    }

    int track_;
    QString title_;
    QString comment_;
    QString genre_;
    QString artist_;
    QString album_;
    QString mrl_;
    QString path_;
    int length_;
    QString lengthString_;
    int year_;
    int disc_;
    int id_;
};
