#include "DirWalker.h"
#include <QDebug>
#include <QDir>
#include <QDirIterator>
#include "exceptions.h"

const QStringList DirWalker::getMusicFileList(const QString& rootPath) const {
    if (rootPath == "" || !QDir(rootPath).exists()) {
        throw DirectoryNotFoundError(rootPath.toStdString());
    }

    QDir rootDir(rootPath);
    QDirIterator it(rootDir, QDirIterator::Subdirectories);
    QStringList fileList;

    while (it.hasNext()) {
        QString line = it.next();
        qDebug() << line;

        if (isMusicFile(line)) {
            fileList << line;
        }
    }

    return fileList;
}

bool DirWalker::isMusicFile(const QString& val) const {
    if (val.endsWith(".mp3")) {
        return true;
    }

    if (val.endsWith(".m4a")) {
        return true;
    }

    if (val.endsWith(".ogg")) {
        return true;
    }

    return false;
}