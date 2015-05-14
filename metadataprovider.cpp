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

MetaDataProvider::MetaDataProvider(QQuickItem *parent) : QQuickItem(parent) {}

QVector<QString> MetaDataProvider::metaData(const QUrl &path) const
{
    QString line(path.path());
    TagLib::FileRef f(line.toLocal8Bit().data());
    QVector<QString> retval(11);
    TimeConverter tc;

    // FIXME: If a tag is corrupt/generates a warning, fix it.
    // This should probably be configurable.
    if (!f.isNull() && f.tag()) {
        TagLib::Tag *tag = f.tag();
        retval[0] = QString::fromUtf8(tag->album().toCString(true));
        retval[1] = QString::fromUtf8(tag->artist().toCString(true));
        retval[2] = QString::fromUtf8(tag->comment().toCString(true));
        retval[3] = QString::fromUtf8(tag->genre().toCString(true));
        retval[4] = QString::number(f.audioProperties()->length());
        // Clearing the timeconverter and get the time as displayable string.
        tc.clear();
        tc.setSeconds(f.audioProperties()->length());
        retval[5] = tc.toString();
        retval[6] = path.toString();
        retval[7] = line;
        retval[8] = QString::fromUtf8(tag->title().toCString(true));
        retval[9] = QString::number(tag->track());
        retval[10] = QString::number(tag->year());
    }

    return retval;
}

QJsonObject MetaDataProvider::metaDataAsJson(const QUrl &path) const
{
    auto tmp = metaData(path);
    QString line(path.path());
    QVariantMap retval;
    retval.insert("album", tmp.at(0));
    retval.insert("artist", tmp.at(1));
    retval.insert("comment", tmp.at(2));
    retval.insert("genre", tmp.at(3));
    retval.insert("length", tmp.at(4).toInt());
    retval.insert("lengthString", tmp.at(5));
    retval.insert("mrl", tmp.at(6));
    retval.insert("path", tmp.at(7));
    retval.insert("title", tmp.at(8));
    retval.insert("track", tmp.at(9).toInt());
    retval.insert("year", tmp.at(10).toInt());

    return QJsonObject::fromVariantMap(retval);
}
