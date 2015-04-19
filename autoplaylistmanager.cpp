#include "autoplaylistmanager.h"

#include <QFileInfoList>
#include <QDir>

AutoPlaylistManager::AutoPlaylistManager(QObject *parent) : QObject(parent)
{
    setAutoPlaylistNames(getPlaylistNames());
    Config::ensureDir(getPath("dummy"));

    watcher_.addPath(Config::AUTOPLAYLISTDIR);
    connect(&watcher_,
            &QFileSystemWatcher::directoryChanged,
            this,
            &AutoPlaylistManager::handleDirChange);
}

AutoPlaylistManager::~AutoPlaylistManager()
{
}

// This requires explanation.
// Originally I had the playlistNames property just READ getPlaylistNames()
// without a write method. I emitted playlistNamesChanged in writePlaylist()
// and deletePlaylist. The signal emission in deletePlaylist would reliably
// segfault. I then added WRITE setPlaylistNames to the property and only
// emitted the signal there. Same endresult, writePlaylist fine, deletePlaylist
// segfault. So now I listen to directory changes and emit the signal then.
// No more segfaults.
void AutoPlaylistManager::handleDirChange()
{
    setAutoPlaylistNames(getPlaylistNames());
    watcher_.addPath(Config::AUTOPLAYLISTDIR);
}

QStringList AutoPlaylistManager::autoPlaylistNames()
{
    return autoPlaylistNames_;
}

void AutoPlaylistManager::setAutoPlaylistNames(const QStringList &names)
{
    autoPlaylistNames_ = names;
    emit autoPlaylistNamesChanged();
}

QJsonArray AutoPlaylistManager::getAutoPlaylist(const QString name) const
{
    QJsonArray retval;
    retval = Config::loadJsonFile(getPath(name)).value(name).toArray();
    return retval;
}

void AutoPlaylistManager::saveAutoPlaylist(const QString &name,
                                           const QJsonArray &args)
{
    QList<AutoPlaylistObject> list;

    for (int i = 0; i < args.count(); ++i) {
        list.append(AutoPlaylistObject(args[i].toObject()));
    }

    saveAutoPlaylist(name, list);
}

void AutoPlaylistManager::deleteAutoPlaylist(const QString &name)
{
    Q_ASSERT(QStringList(autoPlaylistNames()).contains(name));
    QFile::remove(getPath(name));
}

QList<AutoPlaylistObject> AutoPlaylistManager::jsonToApo(const QJsonArray &args)
{
    QList<AutoPlaylistObject> list;

    for (int i = 0; i < args.count(); ++i) {
        list.append(AutoPlaylistObject(args[i].toObject()));
    }

    return list;
}

void AutoPlaylistManager::saveAutoPlaylist(
    const QString &name, const QList<AutoPlaylistObject> &args) const
{
    QJsonArray jarr;

    for (auto itr = args.begin(); itr != args.end(); ++itr) {
        jarr.append((*itr).toJson());
    }

    QJsonObject obj;
    obj.insert(name, jarr);

    Config::saveJsonFile(getPath(name), obj);
}

QString AutoPlaylistManager::getPath(const QString &name) const
{
    return Config::AUTOPLAYLISTDIR + "/" + name + "."
            + LalaTypes::AUTOPLAYLISTSUFFIX;
}

QStringList AutoPlaylistManager::getPlaylistNames() const
{
    QDir d(Config::AUTOPLAYLISTDIR);
    QStringList filters;
    filters << QString("*.") + LalaTypes::AUTOPLAYLISTSUFFIX;
    d.setNameFilters(filters);
    QStringList names;
    QFileInfoList entries = d.entryInfoList();

    for (int i = 0; i < entries.length(); ++i) {
        names.append(entries[i].baseName());
    }

    return names;
}
