#include "Database.h"

#include <QDebug>

#include "config.h"

Database& Database::getInstance() {
    static Database instance;
    return instance;
}

QStringList Database::tables() {
    return db.tables();
}

QSqlQuery Database::exec(const QString& query) {
    return db.exec(query);
}

void Database::transaction() {
    db.transaction();
}

void Database::commit() {
    db.commit();
}

Database::Database() {
    qDebug() << "Database::Database()";
    db = QSqlDatabase::database(Config::MAINDB_NAME);
}
