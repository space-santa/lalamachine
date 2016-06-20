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

#include <QObject>
#include <QJsonObject>
#include <QJsonArray>

class Config : public QObject
{
    Q_OBJECT

    // clang-format off
    Q_PROPERTY(double volume READ volume WRITE setVolume NOTIFY volumeChanged)

    Q_PROPERTY(QString libPath
               READ libPath
               WRITE setLibPath
               NOTIFY libPathChanged)

    Q_PROPERTY(QString lastPlaylist
               READ lastPlaylist
               WRITE setLastPlaylist
               NOTIFY lastPlaylistChanged)

    Q_PROPERTY(QJsonArray playlistColumns
               READ playlistColumns
               WRITE setPlaylistColumns
               NOTIFY playlistColumnsChanged)
    // clang-format on

public:
    explicit Config(QObject *parent = 0);

    static const QString LALADIR;
    static const QString CONFIGPATH;
    static const QString PLAYLISTDIR;
    static const QString AUTOPLAYLISTDIR;
    static const QString MUSICLIBDB;

    Q_INVOKABLE void saveConfig();
    void loadConfig();

    void setVolume(double val);
    double volume();

    void setPlaylistColumns(const QJsonArray &list);
    QJsonArray playlistColumns();

    void setLibPath(const QString &path);
    QString libPath() const;

    void setLastPlaylist(const QString &name);
    QString lastPlaylist() const;

    static QJsonObject loadJsonFile(const QString &path);
    static void saveJsonFile(const QString &path, const QJsonObject &obj);
    static void ensureDir(const QString &path);

signals:
    void volumeChanged();
    void libPathChanged();
    void lastPlaylistChanged();
    void playlistColumnsChanged();

public slots:

private:
    QJsonObject config_;
};
