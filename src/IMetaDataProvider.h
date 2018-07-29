#pragma once

#include <QJsonObject>
#include <QUrl>

class IMetaDataProvider {
public:
    virtual ~IMetaDataProvider(){};
    virtual QJsonObject metaData(const QUrl& path) const = 0;
};