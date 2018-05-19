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
#include <QFile>

#include <fileref.h>
#include <id3v2tag.h>
#include <mpegfile.h>
#include <tag.h>

#include "exceptions.h"
#include "tags.h"
#include "timeconverter.h"

MetaDataProvider::MetaDataProvider() {
}

uint MetaDataProvider::discNumberOfMp3(const QString& path) {
    TagLib::MPEG::File f(path.toStdString().c_str());
    if (f.ID3v2Tag()) {
        TagLib::ID3v2::FrameList l = f.ID3v2Tag()->frameListMap()["TPOS"];
        if (!l.isEmpty()) {
            QString tmp = TStringToQString(l.front()->toString());
            uint retval;
            auto list = tmp.split('/');
            retval = list.first().toInt();
            return retval;
        }
    }

    return 1;
}

Tags MetaDataProvider::metaData(const QUrl& path) const {
    QString tmp = path.toLocalFile();
    TagLib::FileRef f(QFile::encodeName(tmp).constData(), true, TagLib::AudioProperties::Accurate);

    if (!f.isNull() && f.tag()) {
        TimeConverter tc;
        TagLib::Tag* tag = f.tag();
        tc.clear();
        tc.setSeconds(f.audioProperties()->length());

        Tags tags(tag, tmp, tmp, f.audioProperties()->length(), tc.toString(), discNumberOfMp3(tmp));
        return tags;
    }

    throw NoMetaDataException(path.toString().toStdString());
}
