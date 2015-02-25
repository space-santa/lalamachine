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
    connect(this, &MusicLib::displayLibChanged,
            this, &MusicLib::genreListChanged);
    connect(this, &MusicLib::displayLibChanged,
            this, &MusicLib::artistListChanged);
    connect(this, &MusicLib::displayLibChanged,
            this, &MusicLib::albumListChanged);
    readLibFile();

    if (lib_.isEmpty()) {
        scannerThread_.start();
    }
}

MusicLib::~MusicLib()
{
    scannerThread_.quit();
    scannerThread_.wait(5000);
    qDebug() << "MusicLib::~MusicLib()" << displayLib();
}

QJsonObject MusicLib::musicLib() const
{
    return lib_;
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

            if (checkVal(genreFilter(), tmp.value("genre").toString()) &&
                checkVal(artistFilter(), tmp.value("artist").toString()) &&
                checkVal(albumFilter(), tmp.value("album").toString())) {
                retVal.insert(itr.key(), tmp);
            }
        }
    }

    return retVal;
}

QString MusicLib::libPath() const
{
    return libPath_;
}

void MusicLib::setLibPath(const QString &path)
{
    libPath_ = path;
    emit libPathChanged();
    emit startScan(libPath_);
}

QString MusicLib::genreFilter() const
{
    return genreFilter_;
}

void MusicLib::setGenreFilter(const QString &val)
{
    genreFilter_ = val;
    emit genreFilterChanged();
    emit displayLibChanged();
}

QString MusicLib::artistFilter() const
{
    return artistFilter_;
}

void MusicLib::setArtistFilter(const QString &val)
{
    artistFilter_ = val;
    emit artistFilterChanged();
    emit displayLibChanged();
}

QString MusicLib::albumFilter() const
{
    return albumFilter_;
}

void MusicLib::setAlbumFilter(const QString &val)
{
    genreFilter_ = val;
    emit albumFilterChanged();
    emit displayLibChanged();
}

QStringList MusicLib::getList(const QString &what) const
{
    Q_ASSERT(what == "genre" || what == "artist" || what == "album");
    QStringList retVal;
    QJsonObject tmplib = displayLib();
    QJsonObject::const_iterator itr;

    for (itr = tmplib.constBegin(); itr != tmplib.constEnd(); ++itr) {
        if (itr.value().isObject()) {
            QJsonObject tmpo = itr.value().toObject();
            QString tmps = tmpo.value(what).toString();

            if (!retVal.contains(tmps)) {
                retVal << tmps;
            }
        }
    }

    return retVal;
}

QStringList MusicLib::genreList() const
{
    QStringList tmp;

    if (genreFilter() != "") {
        tmp << genreFilter();
    }
    else {
        tmp = getList(QString("genre"));
    }

    return tmp;
}

QStringList MusicLib::artistList() const
{
    QStringList tmp;

    if (artistFilter() != "") {
        tmp << artistFilter();
    }
    else {
        tmp = getList(QString("artist"));
    }

    return tmp;
}

QStringList MusicLib::albumList() const
{
    QStringList tmp;

    if (albumFilter() != "") {
        tmp << albumFilter();
    }
    else {
        tmp = getList(QString("album"));
    }

    return tmp;
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
