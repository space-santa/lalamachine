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

QJsonArray AutoPlaylistManager::loadAutoPlaylist(const QString &name) const
{
    return Config::loadJsonFile(getPath(name)).value(name).toArray();
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
