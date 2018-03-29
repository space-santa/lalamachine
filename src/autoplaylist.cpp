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

#include <QFile>
#include <QSqlDatabase>
#include <QSqlError>
#include <QSqlQuery>

#include "config.h"
#include "model.h"
#include "musiclib.h"

AutoPlaylist::AutoPlaylist(const QString& name, QObject* parent) : QObject(parent), name_(name) {
    Q_ASSERT(!name_.isEmpty());
    db_ = QSqlDatabase::addDatabase("QSQLITE", name_);
    db_.setDatabaseName(Config::MUSICLIBDB);
    load();
}

AutoPlaylist::~AutoPlaylist() {}

QString AutoPlaylist::name() {
    return name_;
}

void AutoPlaylist::save() {
    if (name_.isEmpty()) {
        return;
    }

    QJsonArray jarr;

    for (QList<AutoPlaylistObject>::iterator itr = apos_.begin(); itr != apos_.end(); ++itr) {
        jarr.append((*itr).toJson());
    }

    QJsonObject obj;
    obj.insert(name_, jarr);

    Config::saveJsonFile(getPath(name_), obj);
}

void AutoPlaylist::deleteList() {
    if (name_.isEmpty()) {
        return;
    }

    QFile::remove(getPath(name_));
    name_ = "";
}

void AutoPlaylist::load() {
    if (name_.isEmpty()) {
        return;
    }

    fromJson(Config::loadJsonFile(getPath(name_)).value(name_).toArray());
}

void AutoPlaylist::fromJson(const QJsonArray& arr) {
    clear();
    for (int i = 0; i < arr.count(); ++i) {
        addApo(AutoPlaylistObject(arr[i].toObject()));
    }
}

QJsonArray AutoPlaylist::toJson() const {
    QJsonArray arr;
    for (QList<AutoPlaylistObject>::const_iterator itr = apos_.constBegin(); itr != apos_.constEnd(); ++itr) {
        arr.append((*itr).toJson());
    }

    return arr;
}

QString AutoPlaylist::getPath(const QString& name) const {
    return Config::AUTOPLAYLISTDIR + "/" + name + "." + LalaTypes::AUTOPLAYLISTSUFFIX;
}

void AutoPlaylist::addApo(const AutoPlaylistObject& apo) {
    apos_.append(apo);
    save();
    emit trackListChanged();
}

void AutoPlaylist::clear() {
    apos_.clear();
}

QJsonArray AutoPlaylist::trackList() {
    db_.open();
    // Processing the QSqlResult imediately before closing the dbase.
    QJsonArray result = Model::queryResultToJson(db_.exec(toQuery())).second;
    // WARNING: The dbase must not be closed before the QSqlResult that db.exec
    // returns is processed.
    db_.close();
    return result;
}

QString AutoPlaylist::toQuery() const {
    QString query("SELECT * FROM musiclib WHERE ");
    bool first = true;
    for (QList<AutoPlaylistObject>::const_iterator itr = apos_.constBegin(); itr != apos_.constEnd(); ++itr) {
        query.append((*itr).toQuery(first));
        first = false;
    }
    query.append(" ORDER BY artist, album, track");

    return query.simplified();
}
