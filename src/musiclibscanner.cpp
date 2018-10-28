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

#include "musiclibscanner.h"

#include <QDebug>
#include <QRegularExpression>
#include <QFile>

#include "config.h"

#ifdef _WIN32
#include <Windows.h>
#endif

MusicLibScanner::MusicLibScanner(QObject* parent) : QObject(parent) {
    connect(&process_,
            QOverload<int, QProcess::ExitStatus>::of(&QProcess::finished),
            [=](int exitCode, QProcess::ExitStatus exitStatus) { emit scanFinished(); });
    
	connect(&process_,
			&QProcess::errorOccurred,
			[=]() { emit scanFinished(); });
 }

void MusicLibScanner::scanLib(const QString& path) {
    process_.start(execPath(), QStringList() << Config::MUSICLIBDB + ".new" << path);
}

QString MusicLibScanner::execPath() {
#ifdef _WIN32
    auto hModule = GetModuleHandleW(NULL);
    WCHAR modulePath[MAX_PATH];
    GetModuleFileNameW(hModule, modulePath, MAX_PATH);
    QString execPath = QString::fromWCharArray(modulePath);
    execPath.remove(QRegularExpression("lalamachine.exe$"));
    return execPath + "/ScannerCLI/ScannerCLI.exe";
#else
    throw std::runtime_error("This must not happen. This class only makes sense on Windows.");
#endif
}