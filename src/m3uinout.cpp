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

#include "m3uinout.h"

#include <QStandardPaths>
#include <QDir>
#include <QJsonDocument>
#include <QJsonArray>

#include "config.h"

M3uInOut::M3uInOut(QQuickItem *parent) : QQuickItem(parent)
{
    setPlaylistNames(getPlaylistNames());
    QDir dir(Config::PLAYLISTDIR);
    dir.mkpath(Config::PLAYLISTDIR);

    watcher_.addPath(Config::PLAYLISTDIR);
    connect(&watcher_,
            &QFileSystemWatcher::directoryChanged,
            this,
            &M3uInOut::handleDirChange);
}

// This requires explanation.
// Originally I had the playlistNames property just READ getPlaylistNames()
// without a write method. I emitted playlistNamesChanged in writePlaylist()
// and deletePlaylist. The signal emission in deletePlaylist would reliably
// segfault. I then added WRITE setPlaylistNames to the property and only
// emitted the signal there. Same endresult, writePlaylist fine, deletePlaylist
// segfault. So now I listen to directory changes and emit the signal then.
// No more segfaults.
void M3uInOut::handleDirChange()
{
    setPlaylistNames(getPlaylistNames());
    watcher_.addPath(Config::PLAYLISTDIR);
}

void M3uInOut::writePlaylist(const QString &name, const QJsonArray &json) const
{
    if (name.isEmpty()) return;

    QFile file(m3uPath(name));

    if (!file.open(QIODevice::WriteOnly | QIODevice::Text)) {
        qWarning() << "Couldn't write playlist" << name;
        return;
    }

    QTextStream out(&file);
    QJsonDocument doc(json);
    out << doc.toJson();
}

QJsonArray M3uInOut::readPlaylist(const QString &name) const
{
    QFile file(m3uPath(name));
    QJsonArray retval;

    if (!file.open(QIODevice::ReadOnly | QIODevice::Text)) {
        qWarning() << "Couldn't read playlist" << name;
    }

    QJsonDocument doc = QJsonDocument::fromJson(file.readAll());

    if (doc.isArray()) {
        retval = doc.array();
    } else {
        file.close();
        file.open(QIODevice::ReadOnly | QIODevice::Text);

        QTextStream in(&file);

        retval.append("OLDFORMAT");

        while (!in.atEnd()) {
            retval.append(in.readLine());
        }
    }

    return retval;
}

QStringList M3uInOut::getPlaylistNames() const
{
    QDir d(Config::PLAYLISTDIR);
    QStringList filters;
    filters << "*.m3u";
    d.setNameFilters(filters);
    QStringList names;
    QFileInfoList entries = d.entryInfoList();

    for (int i = 0; i < entries.length(); ++i) {
        if (entries[i].baseName() != "cs1m090") {
            names.append(entries[i].baseName());
        }
    }

    return names;
}

QString M3uInOut::m3uPath(const QString &name) const
{
    return Config::PLAYLISTDIR + "/" + name + ".m3u";
}

void M3uInOut::deletePlaylist(const QString &name) const
{
    Q_ASSERT(QStringList(playlistNames()).contains(name));
    QFile::remove(m3uPath(name));
}

void M3uInOut::setPlaylistNames(const QStringList &list)
{
    playlistNames_ = list;
    emit playlistNamesChanged();
}

QStringList M3uInOut::playlistNames() const { return playlistNames_; }