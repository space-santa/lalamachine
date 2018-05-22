#include "DatabaseSetup.h"
#include <QSqlDatabase>
#include "config.h"

DatabaseSetup::DatabaseSetup() {
    auto db = QSqlDatabase::addDatabase("QSQLITE");
    db.setDatabaseName(Config::MUSICLIBDB);
    db.open();
}

DatabaseSetup::~DatabaseSetup() {
    QSqlDatabase::database().close();
}
