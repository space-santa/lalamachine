#include "Database.h"

#include "config.h"

Database& Database::getInstance() {
    static Database instance;
    return instance;
}

QStringList Database::tables() {
    std::lock_guard<std::mutex> guard(mutex);
    return db.tables();
}

QSqlQuery Database::exec(const QString& query) {
    std::lock_guard<std::mutex> guard(mutex);
    QSqlQuery result = db.exec(query);
    return result;
}

void Database::transaction() {
    std::lock_guard<std::mutex> guard(mutex);
    db.transaction();
}

void Database::commit() {
    std::lock_guard<std::mutex> guard(mutex);
    db.commit();
}

Database::Database() {
    db = QSqlDatabase::addDatabase("QSQLITE", Config::MAINDB_NAME);
    db.setDatabaseName(Config::MUSICLIBDB);
    db.open();
}
