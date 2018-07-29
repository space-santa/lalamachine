#pragma once

#include <QMutex>
#include <QMutexLocker>
#include <QObject>

#include "DirWalker.h"
#include "ScannerDB.h"
#include "metadataprovider.h"
#include "musiclibscanner.h"

class ScannerWorker : public QObject {
    Q_OBJECT
    std::unique_ptr<MusicLibScanner> scanner;
    QMutex mutex_;

public:
    ScannerWorker()
        : scanner(new MusicLibScanner(std::unique_ptr<IScannerDB>(new ScannerDB()),
                                      std::unique_ptr<IDirWalker>(new DirWalker()),
                                      std::unique_ptr<IMetaDataProvider>(new MetaDataProvider()))) {
    }
public slots:
    void doScan(const QString& path) {
        QMutexLocker lock(&mutex_);
        scanner->scanLib(path);
        emit scanFinished();
    }

signals:
    void scanFinished();
};