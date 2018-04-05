#include "model.test.mock.h"

#include <QDebug>

bool QueryResultMock::next() {
    hasNext = !hasNext;
    return hasNext;
}

QVariant QueryResultMock::value(const QString& val) {
    Q_UNUSED(val)
    return retval;
}

void QueryResultMock::first() {
}

std::unique_ptr<IQueryResult> MainDBMock::exec(const QString& query) {
    execQueries << query;
    auto* rawRetval = new QueryResultMock();
    rawRetval->retval = queryRetval;
    auto uniqueRetval = std::unique_ptr<IQueryResult>(rawRetval);
    return uniqueRetval;
}

QStringList MainDBMock::tables() {
    return tableList;
}

void MainDBMock::transaction() {
}

void MainDBMock::commit() {
}