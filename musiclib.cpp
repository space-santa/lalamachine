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
    : QQuickItem(parent)
{
    readLibFile();
    // This moveToThread is making the thread the parent of the scanner_.
    // Therefor it is vital that the scanner_ is a raw pointer, or double free
    // happens.
    scanner_->moveToThread(&scannerThread_);
    connect(&scannerThread_, &QThread::finished, scanner_, &QObject::deleteLater);
    connect(this, &MusicLib::startScan, scanner_, &MusicLibScanner::scanLib);
    connect(scanner_, &MusicLibScanner::scanComplete, this,
            &MusicLib::scanFinished);
    connect(this, &MusicLib::musicLibChanged, this, &MusicLib::writeLibFile);
    scannerThread_.start();
}

MusicLib::~MusicLib()
{
    scannerThread_.quit();
    scannerThread_.wait();
    qDebug() << "MusicLib::~MusicLib()" << displayLib();
}

void MusicLib::readLibFile()
{
    lib_ = Config::loadJsonFile(Config::MUSICLIBPATH);
}

void MusicLib::writeLibFile()
{
    Config::saveJsonFile(Config::MUSICLIBPATH, lib_);
}

QJsonObject MusicLib::displayLib() const
{
    QJsonObject retVal;
    QJsonObject::const_iterator itr;

    for (itr = lib_.constBegin(); itr != lib_.constEnd(); ++itr) {
        if (itr.value().isObject()) {
            QJsonObject tmp = itr.value().toObject();

            if (checkVal(genre(), tmp.value("genre").toString()) &&
                checkVal(artist(), tmp.value("artist").toString()) &&
                checkVal(album(), tmp.value("album").toString()))
            {
                retVal.insert(itr.key(), tmp);
            }
        }
    }

    return retVal;
}

void MusicLib::scanFinished(const QJsonObject &lib)
{
    lib_ = lib;
    emit musicLibChanged();
    emit displayLibChanged();
}

bool MusicLib::checkVal(const QString &check, const QString &val) const
{
    return check == "" || check == val;
}
