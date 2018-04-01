#pragma once

#include <QSqlDatabase>
#include "IMainDB.h"

class MainDB : public IMainDB {
public:
    MainDB();
    ~MainDB();
    QSqlQuery exec(const QString& query);
    QStringList tables();
    void transaction();
    void commit();

private:
    QSqlDatabase db;
};