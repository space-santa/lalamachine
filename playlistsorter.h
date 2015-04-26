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

#ifndef PLAYLISTSORTER_H
#define PLAYLISTSORTER_H

#include <QQuickItem>
#include <QJsonObject>
#include <QJsonArray>

class PlaylistSorter : public QQuickItem
{
    Q_OBJECT
public:
    enum SortWhat { TRACK, TITLE, COMMENT, LENGTH, GENRE, ARTIST };
    Q_ENUMS(SortWhat)

    enum SortHow { ASCENDING, DESCENDING };
    Q_ENUMS(SortHow)

    PlaylistSorter();
    ~PlaylistSorter();

    Q_INVOKABLE QJsonArray
    sort(QJsonArray list, SortWhat what, SortHow how) const;

signals:

public slots:

private:
    // bool titleAscending(QJsonValue i, QJsonValue j);
};

#endif  // PLAYLISTSORTER_H
