#include "track.h"
#include "musiclib.h"

Track::Track() {
}

Track::Track(const QString& track,
             const QString& title,
             const QString& comment,
             const QString& genre,
             const QString& artist,
             const QString& album,
             const QString& mrl,
             const QString& path,
             const QString& length,
             const QString& lengthString,
             const QString& year,
             const QString& id)
    : track_(track.toInt()),
      title_(title),
      comment_(comment),
      genre_(genre),
      artist_(artist),
      album_(album),
      mrl_(mrl),
      path_(path),
      length_(length.toInt()),
      lengthString_(lengthString),
      year_(year.toInt()),
      id_(id.toInt()) {
}

Track::Track(const QJsonObject& json) {
    fromJson(json);
}

void Track::fromJson(const QJsonObject& json) {
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

QJsonObject Track::toJson() const {
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
