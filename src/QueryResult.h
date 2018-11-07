#pragma once

#include <QSqlQuery>

#include "IQueryResult.h"

class QueryResult : public IQueryResult {
public:
    QueryResult(const QSqlQuery& query) : query(query) {
    }

    bool next() {
        return query.next();
    }
    
	QVariant value(const QString& val) {
        return query.value(val);
    }
    
	void first() {
        query.first();
    }

private:
    QSqlQuery query;
};