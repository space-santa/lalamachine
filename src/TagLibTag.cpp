#include <QFile>
#include "TagLibTag.h"
#include "timeconverter.h"

TagLibTag::TagLibTag(const QUrl& url) {
    path_ = url.toLocalFile();
	fileRef_ = TagLib::FileRef(QFile::encodeName(path_).constData(), true, TagLib::AudioProperties::Accurate);
    tag_ = fileRef_.tag();
}

QString TagLibTag::album() {
    return QString::fromUtf8(tag_->album().toCString(true));
}

QString TagLibTag::artist() {
    return QString::fromUtf8(tag_->artist().toCString(true));
}

QString TagLibTag::comment() {
    return QString::fromUtf8(tag_->comment().toCString(true));
}

QString TagLibTag::discNumber() {
    TagLib::MPEG::File f(path_.toStdString().c_str());
    if (f.ID3v2Tag()) {
        TagLib::ID3v2::FrameList l = f.ID3v2Tag()->frameListMap()["TPOS"];
        if (!l.isEmpty()) {
            QString tmp = TStringToQString(l.front()->toString());
            uint retval;
            auto list = tmp.split('/');
            retval = list.first().toInt();
            return retval;
        }
    }
    return 1;
}

QString TagLibTag::genre() {
    return QString::fromUtf8(tag_->genre().toCString(true));
}

QString TagLibTag::length() {
    return QString::number(fileRef_.audioProperties()->length());
}

QString TagLibTag::lengthString() {
    TimeConverter tc;
    tc.clear();
    tc.setSeconds(fileRef_.audioProperties()->length());
    return tc.toString();
}

QString TagLibTag::title() {
    return QString::fromUtf8(tag_->title().toCString(true));
}

QString TagLibTag::track() {
    return QString::number(tag_->track());
}

QString TagLibTag::year() {
    return QString::number(tag_->year());
}

QString TagLibTag::path() {
    return path_;
}
