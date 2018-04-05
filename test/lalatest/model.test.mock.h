#pragma once

#include "../../src/IMainDB.h"

class QueryResultMock : public IQueryResult {
public:
    bool next();
    QVariant value(const QString& val);
    void first();

    QVariant retval;
    bool hasNext = false;
};

class MainDBMock : public IMainDB {
public:
    std::unique_ptr<IQueryResult> exec(const QString& query);
    QStringList tables();
    void transaction();
    void commit();

    QStringList tableList;
    QVariant queryRetval;
    QStringList execQueries;
};