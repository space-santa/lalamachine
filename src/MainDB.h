#pragma once

#include "IMainDB.h"
#include <QDebug>
#include <QSqlDatabase>
#include <QSqlError>
#include "QueryResult.h"
#include "config.h"
#include "exceptions.h"

class MainDB : public IMainDB {
public:
    std::unique_ptr<IQueryResult> exec(const QString& query) {
        auto db = QSqlDatabase::database(Config::MAINDBNAME);
        auto result = db.exec(query);
        auto error = result.lastError();

        if (error.isValid()) {
             qWarning() << error.text();
            throw QueryError(error.text().toStdString());
        } else {
            std::unique_ptr<IQueryResult> retval(new QueryResult(result));
            return std::move(retval);
        }
    }
    QStringList tables() {
        return QSqlDatabase::database(Config::MAINDBNAME).tables();
    }
    void transaction() {
        QSqlDatabase::database(Config::MAINDBNAME).transaction();
    }
    void commit() {
        QSqlDatabase::database(Config::MAINDBNAME).commit();
    }
};
