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

#ifndef AUTOPLAYLIST_H
#define AUTOPLAYLIST_H

#include <QObject>
#include <QList>
#include <QJsonArray>

#include "autoplaylistobject.h"

// This class manages one auto playlist.
// It contains a list of AutoPlaylistObjects and can save/load the list.
class AutoPlaylist : public QObject
{
    friend class TestAutoPlaylist;
    Q_OBJECT

public:
    explicit AutoPlaylist(const QString &name, QObject *parent = 0);
    ~AutoPlaylist();

    void load();
    void save();

    void addApo(const AutoPlaylistObject &apo);
    void clear();

    QJsonObject list();
    void setList(const QJsonObject &list);

    // The list of traccks for the player.
    QJsonArray tracklist();

signals:
    void listChanged();

public slots:

private:
    QString name_;
    QList<AutoPlaylistObject> apos_;

    // The JSON representation of this autoplaylist (e.g. for saving).
    QJsonArray toJson();

    QString toQuery();
};

#endif // AUTOPLAYLIST_H
