#pragma once

#include <QObject>

#include <QThread>

#include "ScannerWorker.h"

class ScannerController : public QObject {
    Q_OBJECT
    QThread workerThread;

public:
    ScannerController() {
        ScannerWorker* worker = new ScannerWorker;
        worker->moveToThread(&workerThread);
        connect(&workerThread, &QThread::finished, worker, &QObject::deleteLater);
        connect(this, &ScannerController::scan, worker, &ScannerWorker::doScan);
        connect(worker, &ScannerWorker::scanFinished, this, &ScannerController::scanFinished);
        workerThread.start();
    }
    ~ScannerController() {
        workerThread.quit();
        workerThread.wait();
    }
signals:
    void scan(const QString&);
    void scanFinished();
};