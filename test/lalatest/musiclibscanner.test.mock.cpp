#include "musiclibscanner.test.mock.h"
#include "../../src/exceptions.h"

const QStringList DirWalkerMock::getMusicFileList(const QString& rootPath) const {
    if (getMusicFileListThrows) {
        throw DirectoryNotFoundError("MOCK THROW DIR MISSING");
    }

    if (rootPath.isEmpty()) {
        throw DirectoryNotFoundError("MOCK ROOT EMPTY");
    }

    QStringList retval;
    retval << "/";
    return retval;
}

Tags MetaDataProviderMock::metaData(const QUrl& path) const {
    if (metaDataThrows) {
        throw NoMetaDataException("MOCK THROW METADATA");
    }

    return retval;
}

void ScannerDBMock::open() {
    if (openThrows) {
        throw OpenDatabaseError("MOCK THROW DB OPEN");
    }
}

void ScannerDBMock::transaction() {
}

void ScannerDBMock::addQuery(const Tags& tags) {
    if (addQueryThrows) {
        throw AddQueryError("MOCK THROW DB ADD QUERY");
    }
    Q_UNUSED(tags);
}

void ScannerDBMock::commit() {
}

void ScannerDBMock::close() {
}
