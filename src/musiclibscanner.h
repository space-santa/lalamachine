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

#pragma once

#include <QObject>
#include <memory>

#include "IDirWalker.h"
#include "IMetaDataProvider.h"
#include "IScannerDB.h"
#include "tags.h"

/*!
 * \brief The MusicLibScanner class is the worker class that scans the library
 * in a dedicated thread. After it is moved to its own thread, calling the
 * scanLib() slot will trigger a scan.
 */
class MusicLibScanner : public QObject {
    Q_OBJECT
    friend class MusicLibScannerTest;

public:
    MusicLibScanner(std::unique_ptr<IScannerDB> scanDb,
                    std::unique_ptr<IDirWalker> dirWalker,
                    std::unique_ptr<IMetaDataProvider> metaDataProvider,
                    QObject* parent = 0);

public slots:
    void scanLib(const QString& path);

signals:
    void scanStarted();
    void scanComplete();
    void trackAdded();

private:
    std::unique_ptr<IScannerDB> scanDb;
    std::unique_ptr<IDirWalker> dirWalker;
    std::unique_ptr<IMetaDataProvider> metaDataProvider;
    void addPathsToScannerDB(const QStringList& paths);
};
