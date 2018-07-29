#pragma once

#include <QJsonObject>
#include <QUrl>

class IMetaDataProvider {
public:
    virtual QJsonObject metaData(const QUrl& path) const = 0;
};