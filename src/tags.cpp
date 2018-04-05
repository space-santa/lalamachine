#include "tags.h"
#include <tpropertymap.h>

#include <QDebug>
#include <QString>
#include <QUrl>

Tags::Tags() {
}

Tags::Tags(
    const TagLib::Tag* tag, const QString& path, const QString& mrl, int length, QString lengthString, uint discNumber)
    : album_(QString::fromUtf8(tag->album().toCString(true))),
      artist_(QString::fromUtf8(tag->artist().toCString(true))),
      comment_(QString::fromUtf8(tag->comment().toCString(true))),
      genre_(QString::fromUtf8(tag->genre().toCString(true))),
      length_(QString::number(length)),
      lengthString_(lengthString),
      mrl_(mrl),
      path_(path),
      title_(QString::fromUtf8(tag->title().toCString(true))),
      track_(QString::number(tag->track())),
      year_(QString::number(tag->year())),
      disc_(QString::number(discNumber)) {
}

QJsonObject Tags::toJson() {
    QJsonObject retval;
    retval.insert("album", album_);
    retval.insert("artist", artist_);
    retval.insert("comment", comment_);
    retval.insert("genre", genre_);
    retval.insert("length", length_.toInt());
    retval.insert("lengthString", lengthString_);
    retval.insert("mrl", mrl_);
    retval.insert("path", path_);
    retval.insert("title", title_);
    retval.insert("track", track_.toInt());
    retval.insert("year", year_.toInt());
    retval.insert("disc", disc_.toInt());

    return retval;
}

// For now my definition of a valid track is that it must have a length and a
// title. The length is obvious, we can't play something that has no lenght.
// While technically not necessary I also want the title, because I think
// a track should have a name.
bool Tags::isValid() {
    return length_.toInt() > 0 && !title_.isEmpty();
}
