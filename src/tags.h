#pragma once

#include <QJsonObject>
#include <QString>
#include <QUrl>

#include <memory>

#include "ITag.h"

class Tags {
public:
    Tags();
    Tags(std::unique_ptr<ITag> tag);

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
    QString disc_;

    QJsonObject toJson();

    bool isValid();
};
