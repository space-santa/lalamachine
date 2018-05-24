#include "DatabaseSetup.h"
#include <QSqlDatabase>
#include <QString>
#include "config.h"

DatabaseSetup::DatabaseSetup() {
    add(Config::AUTODBNAME);
    add(Config::MAINDBNAME);
    add(Config::SCANNERDBNAME);
    add(Config::DISPLAYDBNAME);
}

void DatabaseSetup::add(const QString& name) {
    auto db = QSqlDatabase::addDatabase("QSQLITE", name);
    db.setDatabaseName(Config::MUSICLIBDB);
    db.open();
}

DatabaseSetup::~DatabaseSetup() {
    QSqlDatabase::database(Config::AUTODBNAME).close();
    QSqlDatabase::database(Config::MAINDBNAME).close();
    QSqlDatabase::database(Config::SCANNERDBNAME).close();
    QSqlDatabase::database(Config::DISPLAYDBNAME).close();
}
