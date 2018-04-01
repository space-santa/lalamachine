#pragma once

#include <QSqlQuery>
#include <QString>
#include <QStringList>

class IMainDB {
public:
    virtual QSqlQuery exec(const QString& query) = 0;
    virtual QStringList tables() = 0;
    virtual void transaction() = 0;
    virtual void commit() = 0;
};