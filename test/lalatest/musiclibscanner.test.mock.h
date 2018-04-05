#pragma once

#include "../../src/IDirWalker.h"
#include "../../src/IMetaDataProvider.h"
#include "../../src/IScannerDB.h"

class DirWalkerMock : public IDirWalker {
public:
    const QStringList getMusicFileList(const QString& rootPath) const;

    bool getMusicFileListThrows = false;
};

class MetaDataProviderMock : public IMetaDataProvider {
public:
    Tags metaData(const QUrl& path) const;

    bool metaDataThrows = false;
    Tags retval;
};

class ScannerDBMock : public IScannerDB {
public:
    void open();
    void transaction();
    void addQuery(const Tags& tags);
    void commit();
    void close();

    bool addQueryThrows = false;
    bool openThrows = false;
};