#include "QmlMetadataProvider.h"
#include "exceptions.h"

#include <QDebug>

QmlMetadataProvider::QmlMetadataProvider(QObject* parent) : QObject(parent) {
}

QJsonObject QmlMetadataProvider::metaDataAsJson(const QUrl& path) const {
    try {
        return metaData(path).toJson();
    } catch (const NoMetaDataException& e) {
        qDebug() << "VUCKING ERROR" << e.what();
    }

    return QJsonObject();
}
