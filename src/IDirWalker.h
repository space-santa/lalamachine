#pragma once

#include <QString>
#include <QStringList>

class IDirWalker {
public:
    virtual const QStringList getMusicFileList(const QString& rootPath) const = 0;
};