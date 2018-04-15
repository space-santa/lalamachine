#include "MainDB.h"
#include <QDebug>
#include <QSqlDatabase>
#include <QSqlError>
#include "QueryResult.h"
#include "config.h"
#include "exceptions.h"

std::unique_ptr<IQueryResult> MainDB::exec(const QString& query) {
    auto db = QSqlDatabase::database(Config::MAINDB_NAME);
    auto result = db.exec(query);
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
    auto db = QSqlDatabase::database(Config::MAINDB_NAME);
    return db.tables();
}

void MainDB::transaction() {
    auto db = QSqlDatabase::database(Config::MAINDB_NAME);
    db.transaction();
}

void MainDB::commit() {
    auto db = QSqlDatabase::database(Config::MAINDB_NAME);
    db.commit();
}
