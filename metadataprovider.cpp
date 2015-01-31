#include "metadataprovider.h"

#include <QDebug>
#include <QString>
#include <QVariantMap>
#include <QJsonObject>

#include "tag.h"
#include "fileref.h"

MetaDataProvider::MetaDataProvider(QQuickItem *parent) :
    QQuickItem(parent)
{
}

QJsonObject MetaDataProvider::metaData(const QUrl &path)
{
    QString line(path.path());
    TagLib::FileRef f(line.toLocal8Bit().data());
    QVariantMap tmpmap;

    if (!f.isNull() && f.tag()) {
        TagLib::Tag *tag = f.tag();
        tmpmap.insert("title",
                      QString::fromUtf8(tag->title().toCString()));
        tmpmap.insert("artist",
                      QString::fromUtf8(tag->artist().toCString()));
        tmpmap.insert("album",
                      QString::fromUtf8(tag->album().toCString()));
        tmpmap.insert("year",
                      QString::number(tag->year()));
        tmpmap.insert("comment",
                      QString::fromUtf8(tag->comment().toCString()));
        tmpmap.insert("track",
                      QString::number(tag->track()));
        tmpmap.insert("genre",
                      QString::fromUtf8(tag->genre().toCString()));
        tmpmap.insert("path", line);
        tmpmap.insert("mrl", path);
    }

    return QJsonObject::fromVariantMap(tmpmap);
}
