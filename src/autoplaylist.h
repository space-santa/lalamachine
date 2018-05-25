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

#pragma once

#include <QJsonArray>
#include <QList>
#include <QObject>

#include "autoplaylistobject.h"

/*!
 * \brief The AutoPlaylist class combines several AutoPlaylistObject s to
 * a list and can save and load that list.
 */
class AutoPlaylist : public QObject {
    friend class TestAutoPlaylist;
    Q_OBJECT

public:
    explicit AutoPlaylist(const QString& name, QObject* parent = 0);
    ~AutoPlaylist();

    QString name();

    void load();
    void save();
    void deleteList();

    void addApo(const AutoPlaylistObject& apo);
    void clear();

    QJsonArray trackList();

    void fromJson(const QJsonArray& arr);
    // The JSON representation of this autoplaylist (e.g. for saving).
    QJsonArray toJson() const;

signals:
    void trackListChanged();

public slots:

private:
    QString name_;
    QList<AutoPlaylistObject> apos_;

    QString toQuery() const;
    QString getPath(const QString& name) const;
};
