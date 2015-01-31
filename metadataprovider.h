#ifndef METADATAPROVIDER_H
#define METADATAPROVIDER_H

#include <QQuickItem>
#include <QJsonObject>

class MetaDataProvider : public QQuickItem
{
    Q_OBJECT
public:
    explicit MetaDataProvider(QQuickItem *parent = 0);

    Q_INVOKABLE QJsonObject metaData(const QUrl &path) const;

    Q_INVOKABLE QString secToMinSec(int sec) const;

signals:

public slots:

};

#endif // METADATAPROVIDER_H
