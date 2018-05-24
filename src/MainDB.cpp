#include "MainDB.h"
#include <QDebug>
#include <QSqlDatabase>
#include <QSqlError>
#include "QueryResult.h"
#include "config.h"
#include "exceptions.h"

std::unique_ptr<IQueryResult> MainDB::exec(const QString& query) {
    auto db = QSqlDatabase::database(Config::MAINDBNAME);
    auto result = db.exec(query);
    auto error = result.lastError();

    if (error.isValid()) {
        throw QueryError(error.text().toStdString());
    } else {
        std::unique_ptr<IQueryResult> retval(new QueryResult(result));
        return std::move(retval);
    }
}

QStringList MainDB::tables() {
    return QSqlDatabase::database(Config::MAINDBNAME).tables();
}

void MainDB::transaction() {
    QSqlDatabase::database(Config::MAINDBNAME).transaction();
}

void MainDB::commit() {
    QSqlDatabase::database(Config::MAINDBNAME).commit();
}
