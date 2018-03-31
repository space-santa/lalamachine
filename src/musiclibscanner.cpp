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

#include "musiclibscanner.h"

#include "config.h"
#include "exceptions.h"
#include "model.h"
#include "musiclib.h"
#include "tags.h"

MusicLibScanner::MusicLibScanner(std::unique_ptr<IScannerDB> scanDb,
                                 std::unique_ptr<IDirWalker> dirWalker,
                                 std::unique_ptr<IMetaDataProvider> metaDataProvider,
                                 QObject* parent)
    : QObject(parent),
      scanDb(std::move(scanDb)),
      dirWalker(std::move(dirWalker)),
      metaDataProvider(std::move(metaDataProvider)) {
}

void MusicLibScanner::scanLib(const QString& path) {
    QStringList fileList;
    try {
        fileList = dirWalker->getMusicFileList(path);
    } catch (const DirectoryNotFoundError& error) {
        qDebug() << error.what();
        return;
    }

    try {
        scanDb->open();
    } catch (const OpenDatabaseError& error) {
        qDebug() << "Can't open dbase..." << error.what();
        return;
    }

    emit scanStarted();
    scanDb->transaction();

    for (const QString& file : fileList) {
        try {
            auto tags = metaDataProvider->metaData(QUrl::fromLocalFile(file));
            scanDb->addQuery(tags);
        } catch (const NoMetaDataException& error) {
            qDebug() << error.what();
            continue;
        } catch (const AddQueryError& error) {
            qDebug() << error.what();
            continue;
        }
    }

    scanDb->commit();
    scanDb->close();
    emit scanComplete();
}
