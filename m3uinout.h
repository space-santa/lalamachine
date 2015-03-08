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

#ifndef M3UINOUT_H
#define M3UINOUT_H

#include <QQuickItem>

class M3uInOut : public QQuickItem
{
    Q_OBJECT
public:
    explicit M3uInOut(QQuickItem *parent = 0);

    Q_INVOKABLE void writePlaylist(const QString &name,
                                   const QStringList files) const;
    Q_INVOKABLE QStringList readPlaylist(const QString &name) const;
    Q_INVOKABLE QStringList getPlaylistNames() const;
    Q_INVOKABLE QString m3uPath(const QString &name) const;
    Q_INVOKABLE void deletePlaylist(const QString &name) const;

signals:

public slots:

private:
};

#endif // M3UINOUT_H
