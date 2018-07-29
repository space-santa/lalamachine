#pragma once

#include <QSqlQuery>
#include <QString>
#include <QStringList>
#include <memory>

#include "IQueryResult.h"

class IMainDB {
public:
    virtual ~IMainDB(){};
    virtual std::unique_ptr<IQueryResult> exec(const QString& query) = 0;
    virtual QStringList tables() = 0;
    virtual void transaction() = 0;
    virtual void commit() = 0;
};
