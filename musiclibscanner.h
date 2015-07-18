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

#include "tags.h"

class MusicLibScanner : public QObject
{
    Q_OBJECT

public:
    MusicLibScanner(QObject *parent = 0);

public slots:
    void scanLib(const QString &path);

signals:
    void scanStarted();
    void scanComplete();
    void trackAdded();

private:
    bool suffixCheck(const QString &val) const;
    QSqlDatabase scanDb_;
    QString getTrackQuery(Tags track, const QString date);
};

#endif  // MUSICLIBSCANNER_H
