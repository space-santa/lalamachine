#include <QFile>
#include "TagLibTag.h"
#include "timeconverter.h"

#include <QDebug>

TagLibTag::TagLibTag(const QUrl& url) {
    path_ = url.toLocalFile();
    fileRef_ = TagLib::FileRef(QFile::encodeName(path_).constData(), true, TagLib::AudioProperties::Accurate);
    tag_ = fileRef_.tag();
}

QString TagLibTag::album() const {
    return QString::fromUtf8(tag_->album().toCString(true));
}

QString TagLibTag::artist() const {
    return QString::fromUtf8(tag_->artist().toCString(true));
}

QString TagLibTag::comment() const {
    return QString::fromUtf8(tag_->comment().toCString(true));
}

QString TagLibTag::discNumber() const {
    TagLib::MPEG::File f(path_.toStdString().c_str());
    if (f.hasID3v2Tag()) {
        TagLib::ID3v2::FrameList l = f.ID3v2Tag()->frameListMap()["TPOS"];
        if (!l.isEmpty()) {
            QString tmp = TStringToQString(l.front()->toString());
            QString retval;
            auto list = tmp.split('/');
            retval = list.first();
            return retval;
        }
    }
    return "1";
}

QString TagLibTag::genre() const {
    return QString::fromUtf8(tag_->genre().toCString(true));
}

QString TagLibTag::length() const {
    return QString::number(fileRef_.audioProperties()->length());
}

QString TagLibTag::lengthString() const {
    TimeConverter tc;
    tc.clear();
    tc.setSeconds(fileRef_.audioProperties()->length());
    return tc.toString();
}

QString TagLibTag::title() const {
    return QString::fromUtf8(tag_->title().toCString(true));
}

QString TagLibTag::track() const {
    return QString::number(tag_->track());
}

QString TagLibTag::year() const {
    return QString::number(tag_->year());
}

QString TagLibTag::path() const {
    return path_;
}
