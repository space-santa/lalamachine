#pragma once

#include "IMainDB.h"

class MainDB : public IMainDB {
public:
    std::unique_ptr<IQueryResult> exec(const QString& query);
    QStringList tables();
    void transaction();
    void commit();
};
