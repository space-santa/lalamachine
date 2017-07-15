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

#include <QFileSystemWatcher>
#include <QJsonArray>
#include <QQuickItem>

/*!
 * \brief The M3uInOut class reads and writes playlists.
 *
 * Originally the playlist were just .m3u files, hence the name of this class.
 * For performance reasons, playlists are now saved as a JSON array of Tag
 * objects.
 */
class M3uInOut : public QQuickItem {
  Q_OBJECT

  // clang-format off
    Q_PROPERTY(QStringList playlistNames
               READ playlistNames
               WRITE setPlaylistNames
               NOTIFY playlistNamesChanged)
  // clang-format on

 public:
  explicit M3uInOut(QQuickItem *parent = 0);

  Q_INVOKABLE void writePlaylist(const QString &name,
                                 const QJsonArray &json) const;
  Q_INVOKABLE QJsonArray readPlaylist(const QString &name) const;
  QStringList getPlaylistNames() const;
  Q_INVOKABLE QString m3uPath(const QString &name) const;
  Q_INVOKABLE void deletePlaylist(const QString &name) const;

  void setPlaylistNames(const QStringList &list);
  QStringList playlistNames() const;

 signals:
  void playlistNamesChanged();

 private slots:
  void handleDirChange();

 private:
  QStringList playlistNames_;
  QFileSystemWatcher watcher_;
};
