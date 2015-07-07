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

#include "autoplaylist.h"

AutoPlaylist::AutoPlaylist(const QString &name, QObject *parent)
    : QObject(parent), name_(name)
{
}

AutoPlaylist::~AutoPlaylist() {}

void AutoPlaylist::addApo(const AutoPlaylistObject &apo)
{
    apos_.append(apo);
}

void AutoPlaylist::clear()
{
    apos_.clear();
}

QJsonObject AutoPlaylist::list() { return QJsonObject(); }

void AutoPlaylist::setList(const QJsonObject &list)
{
    Q_UNUSED(list)
    emit listChanged();
}

QString AutoPlaylist::toQuery()
{
    QString query("SELECT * FROM musiclib WHERE ");
    bool first = true;
    for (auto itr = apos_.constBegin(); itr != apos_.constEnd(); ++itr) {
        query.append((*itr).toQuery(first));
        first = false;
    }
    query.append(" ORDER BY artist, album, track");

    return query.simplified();
}
