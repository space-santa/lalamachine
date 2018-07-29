#include "DatabaseSetup.h"
#include <QSqlDatabase>
#include <QString>
#include "config.h"

DatabaseSetup::DatabaseSetup() {
    add(Config::AUTODBNAME);
    add(Config::MAINDBNAME);
}

void DatabaseSetup::add(const QString& name) {
    auto db = QSqlDatabase::addDatabase("QSQLITE", name);
    db.setDatabaseName(Config::MUSICLIBDB);
    db.open();
}

DatabaseSetup::~DatabaseSetup() {
    QSqlDatabase::database(Config::AUTODBNAME).close();
    QSqlDatabase::database(Config::MAINDBNAME).close();
}
