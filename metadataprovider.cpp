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
#include "tags.h"

MetaDataProvider::MetaDataProvider(QQuickItem *parent) : QQuickItem(parent) {}

// FIXME: Is a vector really the proper return value?
// If so, I think I should add an enum to lalatypes mapping field to integer.
Tags MetaDataProvider::metaData(const QUrl &path) const
{
    QString line(path.path());
    TagLib::FileRef f(line.toLocal8Bit().data());
    TimeConverter tc;

    if (!f.isNull() && f.tag()) {
        TagLib::Tag *tag = f.tag();
        // Clearing the timeconverter and get the time as displayable string.
        tc.clear();
        tc.setSeconds(f.audioProperties()->length());
        return Tags(tag,
                    path,
                    line,
                    f.audioProperties()->length(),
                    tc.toString());
    }

    return Tags();
}

QJsonObject MetaDataProvider::metaDataAsJson(const QUrl &path) const
{
    return metaData(path).toJson();
}
