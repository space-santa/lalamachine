#pragma once

#include <QString>
#include <QSqlDatabase>
#include "config.h"

class DatabaseSetup {
public:
    DatabaseSetup() {
        add(Config::MAINDBNAME);
    }
    ~DatabaseSetup() {
        QSqlDatabase::database(Config::MAINDBNAME).close();
    }

private:
    void add(const QString& name) {
        auto db = QSqlDatabase::addDatabase("QSQLITE", name);
        db.setDatabaseName(Config::MUSICLIBDB);
        db.open();
    }
};
