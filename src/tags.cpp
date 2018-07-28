#include "tags.h"
#include <tpropertymap.h>

#include <QDebug>
#include <QString>
#include <QUrl>

Tags::Tags() {
}

Tags::Tags(std::unique_ptr<ITag> tag)
    : album_(tag->album()),
      artist_(tag->artist()),
      comment_(tag->comment()),
      genre_(tag->genre()),
      length_(tag->length()),
      lengthString_(tag->lengthString()),
      mrl_(tag->path()),
      path_(tag->path()),
      title_(tag->title()),
      track_(tag->track()),
      year_(tag->year()),
      disc_(tag->discNumber()) {
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
