#include "MainDB.h"
#include <QSqlError>
#include "config.h"
#include "exceptions.h"

MainDB::MainDB() {
    db = QSqlDatabase::addDatabase("QSQLITE");
    db.setDatabaseName(Config::MUSICLIBDB);
    db.open();
}

MainDB::~MainDB() {
    db.close();
}

QSqlQuery MainDB::exec(const QString& query) {
    auto retval = db.exec(query);
    auto error = retval.lastError();

    if (error.isValid()) {
        throw QueryError(error.text().toStdString());
    } else {
        return retval;
    }
}

QStringList MainDB::tables() {
    return db.tables();
}

void MainDB::transaction() {
    db.transaction();
}

void MainDB::commit() {
    db.commit();
}