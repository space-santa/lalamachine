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

#include "sysinfo.h"

#include <QDebug>
#include <QSysInfo>

#include "config.h"

QString SysInfo::appVersion() { return QCoreApplication::applicationVersion(); }

QString SysInfo::appName() { return QCoreApplication::applicationName(); }

QString SysInfo::lalaDir() { return Config::LALADIR; }
