#include "musiclib.h"

#include <QMutexLocker>
#include <QDir>
#include <QDirIterator>
#include <QJsonObject>
#include <QUrl>

#include "metadataprovider.h"
#include "config.h"

MusicLibScanner::MusicLibScanner(QObject *parent) :
    QObject(parent)
{
}

void MusicLibScanner::scanLib(const QString &path)
{
    QJsonObject lib;
    MetaDataProvider meta;
    QDir rootDir(path);
    QMutexLocker locker(&mutex_);

    if (!rootDir.exists()) {
        qDebug() << "Dir not found.";
    }
    else {
        QDirIterator it(rootDir, QDirIterator::Subdirectories);

        while (it.hasNext()) {
            QString line = it.next();

            if (suffixCheck(line)) {
                lib.insert(line, meta.metaData(QUrl(line)));
            }
        }
    }

    emit scanComplete(lib);
}

bool MusicLibScanner::suffixCheck(const QString &val) const
{
    if (val.endsWith(".mp3")) {
        return true;
    }

    if (val.endsWith(".m4a")) {
        return true;
    }

    if (val.endsWith(".ogg")) {
        return true;
    }

    return false;
}


MusicLib::MusicLib(QQuickItem *parent)
    : QQuickItem(parent),
      scanner_(new MusicLibScanner(this)),
      scannerThread_(new QThread(this))
{
    scanner_->moveToThread(scannerThread_);
    connect(scannerThread_, &QThread::finished, scanner_, &QObject::deleteLater);
    connect(this, &MusicLib::startScan, scanner_, &MusicLibScanner::scanLib);
    connect(scanner_, &MusicLibScanner::scanComplete, this,
            &MusicLib::scanFinished);
    scannerThread_->start();
}

MusicLib::~MusicLib()
{
    scannerThread_->quit();
    scannerThread_->wait();
}

void MusicLib::scanFinished(const QJsonObject &lib)
{
    lib_ = lib;
    emit musicLibChanged();
}
