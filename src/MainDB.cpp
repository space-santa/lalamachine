#include "MainDB.h"
#include <QDebug>
#include <QSqlDatabase>
#include <QSqlError>
#include "QueryResult.h"
#include "config.h"
#include "exceptions.h"

std::unique_ptr<IQueryResult> MainDB::exec(const QString& query) {
    auto result = QSqlDatabase::database().exec(query);
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
    return QSqlDatabase::database().tables();
}

void MainDB::transaction() {
    QSqlDatabase::database().transaction();
}

void MainDB::commit() {
    QSqlDatabase::database().commit();
}
