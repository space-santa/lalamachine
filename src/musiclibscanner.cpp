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
#include "DirWalker.h"
#include "ScannerDB.h"
#include "metadataprovider.h"

MusicLibScanner::MusicLibScanner(std::unique_ptr<IScannerDB> scanDb,
                                 std::unique_ptr<IDirWalker> dirWalker,
                                 std::unique_ptr<IMetaDataProvider> metaDataProvider)
    : scanDb(std::move(scanDb)), dirWalker(std::move(dirWalker)), metaDataProvider(std::move(metaDataProvider)) {
}

void MusicLibScanner::scan(const QString& path) {
    auto scanner = std::unique_ptr<MusicLibScanner>(
        new MusicLibScanner(std::unique_ptr<IScannerDB>(new ScannerDB()),
                            std::unique_ptr<IDirWalker>(new DirWalker()),
                            std::unique_ptr<IMetaDataProvider>(new MetaDataProvider())));
    scanner->scanLib(path);
}

void MusicLibScanner::addPathsToScannerDB(const QStringList& paths) {
    for (const QString& file : paths) {
        try {
            auto tags = metaDataProvider->metaData(QUrl::fromLocalFile(file));
            scanDb->addQuery(tags);
        } catch (const NoMetaDataException& error) {
            qDebug() << error.what();
            continue;
        }
    }
}

void MusicLibScanner::scanLib(const QString& path) {
    QStringList fileList;

    try {
        fileList = dirWalker->getMusicFileList(path);
    } catch (const DirectoryNotFoundError& error) {
        qDebug() << error.what();
        return;
    }

    scanDb->transaction();
    addPathsToScannerDB(fileList);

    try {
        scanDb->commit();
    } catch (const OpenDatabaseError& error) {
        qDebug() << "Can't open dbase..." << error.what();
        return;
    }
}
