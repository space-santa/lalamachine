#include "metadataprovider.h"

#include <QDebug>
#include <QString>
#include <QVariantMap>
#include <QJsonObject>
#include <QTime>

#include "tag.h"
#include "fileref.h"

MetaDataProvider::MetaDataProvider(QQuickItem *parent) :
    QQuickItem(parent)
{
}

QJsonObject MetaDataProvider::metaData(const QUrl &path) const
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
        tmpmap.insert("length", f.audioProperties()->length());
        tmpmap.insert("lengthString", secToMinSec(f.audioProperties()->length()));
    }

    return QJsonObject::fromVariantMap(tmpmap);
}

QString MetaDataProvider::secToMinSec(int sec) const
{
    Q_ASSERT(sec > 0);
    QTime t(0, 0, 0, 0);
    return t.addSecs(sec).toString("mm:ss");
}
