#pragma once

#include "IDirWalker.h"

class DirWalker : public IDirWalker {
public:
    const QStringList getMusicFileList(const QString& rootPath) const;

private:
    bool isMusicFile(const QString& path) const;
};