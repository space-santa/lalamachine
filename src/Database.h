#pragma once

#include <QMutex>
#include <QSqlDatabase>
#include <QSqlQuery>
#include <mutex>

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
    std::mutex mutex;
};
