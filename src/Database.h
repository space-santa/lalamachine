#pragma once

#include <QSqlDatabase>
#include <QSqlQuery>
#include <QMutex>

class Database {
public:
    static Database& getInstance();
    QStringList tables();
    QSqlQuery exec(const QString& query);
    void transaction();
    void commit();

private:
    Database();
    QSqlDatabase db;
    QMutex mutex;
};
