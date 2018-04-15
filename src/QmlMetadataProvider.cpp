#include "QmlMetadataProvider.h"

QmlMetadataProvider::QmlMetadataProvider(QObject* parent) : QObject(parent) {
}

QJsonObject QmlMetadataProvider::metaDataAsJson(const QUrl& path) const {
    return metaData(path).toJson();
}
