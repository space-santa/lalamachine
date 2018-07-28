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

    QString album();
    QString artist();
    QString comment();
    QString genre();
    QString title();
    QString track();
    QString year();
    QString discNumber();
    QString length();
    QString lengthString();
    QString path();

private:
    QString path_;
    TagLib::FileRef fileRef_;
    TagLib::Tag* tag_;
};