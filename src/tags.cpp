#include "tags.h"

#include <QDebug>

Tags::Tags(std::unique_ptr<ITag> tag) : tag_(std::move(tag)) {
}

QJsonObject Tags::toJson() {
    QJsonObject retval;
    retval.insert("album", tag_->album());
    retval.insert("artist", tag_->artist());
    retval.insert("comment", tag_->comment());
    retval.insert("genre", tag_->genre());
    retval.insert("length", tag_->length().toInt());
    retval.insert("lengthString", tag_->lengthString());
    retval.insert("mrl", tag_->path());
    retval.insert("path", tag_->path());
    retval.insert("title", tag_->title());
    retval.insert("track", tag_->track().toInt());
    retval.insert("year", tag_->year().toInt());
    retval.insert("disc", tag_->discNumber().toInt());

    return retval;
}

// For now my definition of a valid track is that it must have a length and a
// title. The length is obvious, we can't play something that has no lenght.
// While technically not necessary I also want the title, because I think
// a track should have a name.
bool Tags::isValid() {
    return tag_->length().toInt() > 0 && !tag_->title().isEmpty();
}
