#pragma once

#include <QJsonObject>
#include <QString>
#include <QUrl>

#include <memory>

#include "TagLibTag.h"

class Tags {
public:
    Tags(const QUrl& path);

    ~Tags();

    QJsonObject toJson();

    bool isValid();

private:
    TagLibTag tag_;
};
