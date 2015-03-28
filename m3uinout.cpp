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

#include "config.h"

M3uInOut::M3uInOut(QQuickItem *parent) :
    QQuickItem(parent)
{
    QDir dir(Config::PLAYLISTDIR);
    dir.mkpath(Config::PLAYLISTDIR);
}

void M3uInOut::writePlaylist(const QString &name,
                             const QStringList files)
{
    QFile file(m3uPath(name));

    if (file.open(QIODevice::WriteOnly | QIODevice::Text)) {
        QTextStream out(&file);

        for (int i = 0; i < files.length(); ++i) {
            out << files[i];

            if (!files[i].endsWith("\n")) {
                out << "\n";
            }
        }
        emit playlistNamesChanged();
    }
}

QStringList M3uInOut::readPlaylist(const QString &name) const
{
    QFile file(m3uPath(name));
    QStringList retVal;

    if (file.open(QIODevice::ReadOnly | QIODevice::Text)) {
        QTextStream in(&file);

        while (!in.atEnd()) {
            retVal.append("file://" + in.readLine());
        }
    }

    return retVal;
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
    Q_ASSERT(QStringList(getPlaylistNames()).contains(name));
    QFile::remove(m3uPath(name));
}
