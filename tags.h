#ifndef TAGS_H
#define TAGS_H

#include <QString>
#include <QUrl>
#include <QJsonObject>
#include "tag.h"

class Tags
{
public:
    Tags();
    Tags(const TagLib::Tag *tag,
         const QUrl &path,
         const QString &mrl,
         int length,
         QString lengthString);

    QString album_;
    QString artist_;
    QString comment_;
    QString genre_;
    QString length_;
    QString lengthString_;
    QString mrl_;
    QString path_;
    QString title_;
    QString track_;
    QString year_;

    QJsonObject toJson();

    bool isValid();
};

#endif  // TAGS_H
