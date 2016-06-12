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

#ifndef FILEEXPORTER_H
#define FILEEXPORTER_H

#include <QObject>
#include <QThread>

class FileExportWorker : public QObject
{
    Q_OBJECT
public slots:
    void doExport(QString destdir, const QStringList &paths);

signals:
    void finished();
    void updateProgress(int current, int total);

private:
    QString newFileName(int pos, int max, const QString &path) const;
};

class FileExporter : public QObject
{
    Q_OBJECT
public:
    explicit FileExporter(QObject *parent = 0);
    ~FileExporter();

    Q_INVOKABLE void exportPlaylist(QString destdir, const QStringList &paths);

public slots:
    void handleUpdate(int current, int total);

signals:
    void go(QString destdir, const QStringList &paths);
    void started();
    void finished();
    void updateProgress(int val);

private:
    QThread workerThread_;
};

#endif  // FILEEXPORTER_H
