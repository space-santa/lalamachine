#include "MainDB.h"
#include <QDebug>
#include <QSqlError>
#include "Database.h"
#include "QueryResult.h"
#include "config.h"
#include "exceptions.h"

std::unique_ptr<IQueryResult> MainDB::exec(const QString& query) {
    auto result = Database::getInstance().exec(query);
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
    return Database::getInstance().tables();
}

void MainDB::transaction() {
    Database::getInstance().transaction();
}

void MainDB::commit() {
    Database::getInstance().commit();
}
