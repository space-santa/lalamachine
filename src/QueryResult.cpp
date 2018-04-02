#include "QueryResult.h"

QueryResult::QueryResult(const QSqlQuery& query) : query(query) {
}

bool QueryResult::next() {
    return query.next();
}

QVariant QueryResult::value(const QString& val) {
    return query.value(val);
}

void QueryResult::first() {
    query.first();
}