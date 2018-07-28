#pragma once

#include <fileref.h>
#include <id3v2tag.h>
#include <mpegfile.h>
#include <tag.h>

#include <QUrl>

#include "ITag.h"

class TagLibTag : public ITag {
public:
    TagLibTag(const QUrl& url);

    QString album() const;
    QString artist() const;
    QString comment() const;
    QString genre() const;
    QString title() const;
    QString track() const;
    QString year() const;
    QString discNumber() const;
    QString length() const;
    QString lengthString() const;
    QString path() const;

private:
    QString path_;
    TagLib::FileRef fileRef_;
    TagLib::Tag* tag_;
};