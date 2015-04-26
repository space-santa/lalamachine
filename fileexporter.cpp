#include "fileexporter.h"
#include "config.h"

#include <QFile>
#include <QFileInfo>
#include <QString>
#include <QDebug>

FileExporter::FileExporter(QObject *parent) : QObject(parent)
{
    FileExportWorker *worker = new FileExportWorker();
    worker->moveToThread(&workerThread_);
    connect(&workerThread_, &QThread::finished, worker, &QObject::deleteLater);
    connect(this, &FileExporter::go, worker, &FileExportWorker::doExport);
    connect(worker, &FileExportWorker::finished, this, &FileExporter::finished);
    connect(worker,
            &FileExportWorker::updateProgress,
            this,
            &FileExporter::handleUpdate);
    workerThread_.start();
}

FileExporter::~FileExporter()
{
    workerThread_.quit();
    workerThread_.wait();
}

void FileExporter::exportPlaylist(QString destdir, const QStringList &paths)
{
    emit go(destdir, paths);
    emit started();
}

void FileExporter::handleUpdate(int current, int total)
{
    int retval = static_cast<int>(static_cast<double>(current) / total * 100);
    qDebug() << "Updating progress" << retval << current << total;
    emit updateProgress(retval);
}

QString FileExportWorker::newFileName(int pos,
                                      int max,
                                      const QString &path) const
{
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

void FileExportWorker::doExport(QString destdir, const QStringList &paths)
{
    destdir.replace("file://", "");
    Config::ensureDir(destdir);

    for (int i = 0; i < paths.count(); ++i) {
        QString newpath(destdir);
        newpath.append("/");
        newpath.append(newFileName(i, paths.count(), paths[i]));
        QFile::copy(paths[i], newpath);

        // emitting i + 1 for two reasons.
        // 1. to make it work, paths.count is 1 bigger than the last i
        // 2. because it is actually what it is. By the time we emit this
        // signal, the file has already been processed, so +1.
        emit updateProgress(i + 1, paths.count());
    }

    emit finished();
}
