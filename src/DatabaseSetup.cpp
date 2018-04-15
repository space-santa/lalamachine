#include "DatabaseSetup.h"
#include <QSqlDatabase>
#include "config.h"

DatabaseSetup::DatabaseSetup() {
    auto scannerDB = QSqlDatabase::addDatabase("QSQLITE", Config::SCANNERDB_NAME);
    scannerDB.setDatabaseName(Config::MUSICLIBDB);
    scannerDB.open();

    auto mainDB = QSqlDatabase::addDatabase("QSQLITE", Config::MAINDB_NAME);
    mainDB.setDatabaseName(Config::MUSICLIBDB);
    mainDB.open();
}

DatabaseSetup::~DatabaseSetup() {
    QSqlDatabase::database(Config::SCANNERDB_NAME).close();
    QSqlDatabase::database(Config::MAINDB_NAME).close();
}
