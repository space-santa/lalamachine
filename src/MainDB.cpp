#include "MainDB.h"
#include <QSqlError>
#include "QueryResult.h"
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

#include <QDebug>
std::unique_ptr<IQueryResult> MainDB::exec(const QString& query) {
    QSqlQuery result;
    result.exec(query);
    auto error = result.lastError();

    while (result.isActive()) {
        qDebug() << "query" << query << "still active";
    }

    if (error.isValid()) {
        throw QueryError(error.text().toStdString());
    } else {
        std::unique_ptr<IQueryResult> retval(new QueryResult(result));
        return std::move(retval);
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
