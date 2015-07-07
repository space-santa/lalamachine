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
    Config::ensureDir(getPath("dummy"));

    loadPlaylists();
}

AutoPlaylistManager::~AutoPlaylistManager() {}

QString AutoPlaylistManager::currentList() const { return currentList_; }

void AutoPlaylistManager::setCurrentList(const QString &val)
{
    // The name must have been selected from this list, so sanity check.
    Q_ASSERT(autoPlaylistNames().contains(val));
    currentList_ = val;
    emit currentListChanged();
}

QStringList AutoPlaylistManager::autoPlaylistNames()
{
    return playlists_.keys();
}

QJsonArray AutoPlaylistManager::getTracks(const QString name) const
{
    return playlists_.value(name)->trackList();
}

QJsonArray AutoPlaylistManager::getAutoPlaylist(const QString name) const
{
    qDebug() << "list for" << name << playlists_.value(name)->toJson();
    return playlists_.value(name)->toJson();
}

void AutoPlaylistManager::saveAutoPlaylist(const QString &name,
                                           const QJsonArray &args)
{
    AutoPlaylist *tmp = new AutoPlaylist(name, this);
    tmp->fromJson(args);
    tmp->save();
    playlists_.insert(name, tmp);
    emit autoPlaylistNamesChanged();
}

void AutoPlaylistManager::deleteAutoPlaylist(const QString &name)
{
    Q_ASSERT(QStringList(autoPlaylistNames()).contains(name));
    playlists_.value(name)->deleteList();
    playlists_.value(name)->~AutoPlaylist();
    playlists_.remove(name);
    emit autoPlaylistNamesChanged();
}

QList<AutoPlaylistObject> AutoPlaylistManager::jsonToApo(const QJsonArray &args)
{
    QList<AutoPlaylistObject> list;

    for (int i = 0; i < args.count(); ++i) {
        list.append(AutoPlaylistObject(args[i].toObject()));
    }

    return list;
}

QString AutoPlaylistManager::getPath(const QString &name) const
{
    return Config::AUTOPLAYLISTDIR + "/" + name + "."
           + LalaTypes::AUTOPLAYLISTSUFFIX;
}

void AutoPlaylistManager::loadPlaylists()
{
    auto list = getPlaylistNames();
    for (auto itr = list.constBegin(); itr != list.constEnd(); ++itr) {
        AutoPlaylist *tmp = new AutoPlaylist((*itr), this);
        playlists_.insert((*itr), tmp);
    }
    emit autoPlaylistNamesChanged();
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
