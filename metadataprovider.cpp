/*
Copyright 2015 Armin Zirkel

This file is part of lalamachine.

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "metadataprovider.h"

#include <QDebug>
#include <QString>
#include <QVariantMap>
#include <QJsonObject>
#include <QTime>

#include "tag.h"
#include "fileref.h"

#include "timeconverter.h"

MetaDataProvider::MetaDataProvider(QQuickItem *parent) :
    QQuickItem(parent)
{
}

QJsonObject MetaDataProvider::metaData(const QUrl &path) const
{
    QString line(path.path());
    TagLib::FileRef f(line.toLocal8Bit().data());
    QVariantMap tmpmap;
    TimeConverter tc;

    if (!f.isNull() && f.tag()) {
        TagLib::Tag *tag = f.tag();
        tmpmap.insert("title",
                      QString::fromUtf8(tag->title().toCString(true)));
        tmpmap.insert("artist",
                      QString::fromUtf8(tag->artist().toCString(true)));
        tmpmap.insert("album",
                      QString::fromUtf8(tag->album().toCString(true)));
        tmpmap.insert("year",
                      QString::number(tag->year()));
        tmpmap.insert("comment",
                      QString::fromUtf8(tag->comment().toCString(true)));
        tmpmap.insert("track",
                      QString::number(tag->track()));
        tmpmap.insert("genre",
                      QString::fromUtf8(tag->genre().toCString(true)));
        tmpmap.insert("path", line);
        tmpmap.insert("mrl", path);
        tmpmap.insert("length", f.audioProperties()->length());
        // Clearing the timeconverter and get the time as displayable string.
        tc.clear();
        tc.setSeconds(f.audioProperties()->length());
        tmpmap.insert("lengthString", tc.toString());
    }

    return QJsonObject::fromVariantMap(tmpmap);
}
