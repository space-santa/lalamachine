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
#include <QJsonObject>
#include <QString>
#include <QTime>
#include <QVariantMap>

#include <fileref.h>
#include <tag.h>

#include "tags.h"
#include "timeconverter.h"

MetaDataProvider::MetaDataProvider(QObject *parent) : QObject(parent)
{
}

Tags MetaDataProvider::metaData(const QUrl &path)
{
    QString tmp = path.toLocalFile();
    TagLib::FileRef f(tmp.toUtf8().data(),
                      true,
                      TagLib::AudioProperties::Accurate);
    TimeConverter tc;

    if (!f.isNull() && f.tag()) {
        TagLib::Tag *tag = f.tag();
        // Clearing the timeconverter and get the time as displayable string.
        tc.clear();
        tc.setSeconds(f.audioProperties()->length());
        return Tags(tag,
                    tmp,
                    tmp,
                    f.audioProperties()->length(),
                    tc.toString());
    }

    return Tags();
}

QJsonObject MetaDataProvider::metaDataAsJson(const QUrl &path) const
{
    return metaData(path).toJson();
}
