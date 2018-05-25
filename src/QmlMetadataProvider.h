#pragma once

#include <QJsonObject>
#include <QObject>

#include "metadataprovider.h"

class QmlMetadataProvider : public QObject, public MetaDataProvider {
    Q_OBJECT
public:
    QmlMetadataProvider(QObject* parent = 0);
    Q_INVOKABLE QJsonObject metaDataAsJson(const QUrl& path) const;
};
