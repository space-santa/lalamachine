#pragma once

#include <QSqlQuery>

#include "IQueryResult.h"

class QueryResult : public IQueryResult {
public:
    QueryResult(const QSqlQuery& query);
    bool next();
    QVariant value(const QString& val);
    void first();

private:
    QSqlQuery query;
};