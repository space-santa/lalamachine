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

#include "fileexporter.h"
#include "config.h"

#include <QDateTime>
#include <QDebug>
#include <QFile>
#include <QFileInfo>
#include <QString>

FileExporter::FileExporter(QObject *parent) : QObject(parent) {
  FileExportWorker *worker = new FileExportWorker();
  worker->moveToThread(&workerThread_);
  connect(&workerThread_, &QThread::finished, worker, &QObject::deleteLater);
  connect(this, &FileExporter::go, worker, &FileExportWorker::doExport);
  connect(worker, &FileExportWorker::finished, this, &FileExporter::finished);
  connect(worker, &FileExportWorker::updateProgress, this,
          &FileExporter::handleUpdate);
  workerThread_.start();
}

FileExporter::~FileExporter() {
  workerThread_.quit();
  workerThread_.wait();
}

void FileExporter::exportPlaylist(QString destdir, const QStringList &paths) {
  qDebug() << destdir;
  // Firstly, make it a proper path, not a url.
  // destdir.replace("file://", "");
  QUrl url(destdir);

  QString tmp = url.toLocalFile();

  qDebug() << tmp;

  // If the directory exists, create a new one. Else the content is mixed.
  if (QFileInfo::exists(tmp)) {
    // Can't use ISO date format here because Windows doesn't allow : ffs.
    tmp.append("-" + QDateTime::currentDateTime().toString("yyyyMMdd_hhmmss"));
  }
  emit go(tmp, paths);
  emit started();
}

void FileExporter::handleUpdate(int current, int total) {
  int retval = static_cast<int>(static_cast<double>(current) / total * 100);
  qDebug() << "Updating progress" << retval << current << total;
  emit updateProgress(retval);
}

QString FileExportWorker::newFileName(int pos, int max,
                                      const QString &path) const {
  QFileInfo fi(path);
  QString retval("");
  QString posstring(QString::number(pos));
  QString maxstring(QString::number(max));
  int diff = maxstring.length() - posstring.length();

  for (int i = 0; i < diff; ++i) {
    retval.append("0");
  }

  retval.append(posstring);
  retval.append("-");
  retval.append(fi.fileName());

  return retval;
}

void FileExportWorker::doExport(QString destdir, const QStringList &paths) {
  Config::ensureDir(destdir + "/");

  for (int i = 0; i < paths.count(); ++i) {
    QString newpath(destdir);
    newpath.append("/");
    newpath.append(newFileName(i, paths.count(), paths[i]));
    QString source(paths[i]);
    qDebug() << source << newpath;
    QFile::copy(source, newpath);

    // emitting i + 1 for two reasons.
    // 1. to make it work, paths.count is 1 bigger than the last i
    // 2. because it is actually what it is. By the time we emit this
    // signal, the file has already been processed, so +1.
    emit updateProgress(i + 1, paths.count());
  }

  emit finished();
}
