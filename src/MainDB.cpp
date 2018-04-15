#include "MainDB.h"
#include <QSqlDatabase>
#include <QSqlError>
#include "QueryResult.h"
#include "config.h"
#include "exceptions.h"

MainDB::MainDB() {
    auto db = QSqlDatabase::addDatabase("QSQLITE");
    db.setDatabaseName(Config::MUSICLIBDB);
    db.open();
}

MainDB::~MainDB() {
    auto db = QSqlDatabase::database();
    db.close();
}

#include <QDebug>
std::unique_ptr<IQueryResult> MainDB::exec(const QString& query) {
    QSqlQuery result;
    result.exec(query);
    auto error = result.lastError();
    qDebug() << "query" << query << "still active";

    if (error.isValid()) {
        throw QueryError(error.text().toStdString());
    } else {
        std::unique_ptr<IQueryResult> retval(new QueryResult(result));
        return std::move(retval);
    }
}

QStringList MainDB::tables() {
    auto db = QSqlDatabase::database();
    return db.tables();
}

void MainDB::transaction() {
    auto db = QSqlDatabase::database();
    db.transaction();
}

void MainDB::commit() {
    auto db = QSqlDatabase::database();
    db.commit();
}
