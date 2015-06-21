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

#ifndef MUSICLIBSCANNER_H
#define MUSICLIBSCANNER_H

#include <QObject>
#include <QSqlDatabase>
#include <QMutex>
#include <QSharedPointer>

#include "tags.h"

class MusicLibScanner : public QObject
{
    Q_OBJECT

public:
    MusicLibScanner(QObject *parent = 0);

    void setDb(QSqlDatabase *db);

    QSharedPointer<QMutex> mutex() const;
    void setMutex(const QSharedPointer<QMutex> &mutex);

public slots:
    void scanLib(const QString &path);

signals:
    void scanStarted();
    void scanComplete();
    void trackAdded();

private:
    QSharedPointer<QMutex> mutex_;
    bool suffixCheck(const QString &val) const;
    QSqlDatabase scanDb_;
    QString getTrackQuery(Tags track);
};

#endif  // MUSICLIBSCANNER_H
