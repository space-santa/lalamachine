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

#ifndef CDWRITER_H
#define CDWRITER_H

#include <QQuickItem>

#include "systeminteractor.h"

class CdWriter : public QQuickItem
{
    Q_OBJECT

public:
    CdWriter();
    ~CdWriter();

    bool hasK3b() const;

    Q_INVOKABLE bool burnList(const QString &m3uPath) const;

signals:

public slots:

private:
    SystemInteractor *sys_ {new SystemInteractor(this)};

    QStringList parseM3u(const QString &m3uPath) const;
};

#endif // CDWRITER_H
