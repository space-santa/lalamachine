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

#ifndef SYSINFO_H
#define SYSINFO_H

#include <QObject>
#include <QCoreApplication>

class SysInfo : public QObject
{
    Q_OBJECT
public:
    Q_INVOKABLE QString appVersion();
    Q_INVOKABLE QString appName();
    Q_INVOKABLE QString currentCpu();
    Q_INVOKABLE QString kernelType();
    Q_INVOKABLE QString kernelVersion();
    Q_INVOKABLE QString prettyProductName();
    Q_INVOKABLE QString lalaDir();

signals:

public slots:
};

#endif  // SYSINFO_H
