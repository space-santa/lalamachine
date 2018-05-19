#include "DatabaseSetup.h"
#include <QSqlDatabase>
#include "config.h"

DatabaseSetup::DatabaseSetup() {
    auto scannerDB = QSqlDatabase::addDatabase("QSQLITE", Config::SCANNERDB_NAME);
    scannerDB.setDatabaseName(Config::MUSICLIBDB);
    scannerDB.open();
}

DatabaseSetup::~DatabaseSetup() {
    QSqlDatabase::database(Config::SCANNERDB_NAME).close();
}
