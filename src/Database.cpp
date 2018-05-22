#include "Database.h"

#include <QDebug>
#include <QMutexLocker>

#include "config.h"

Database& Database::getInstance() {
    static Database instance;
    return instance;
}

QStringList Database::tables() {
    QMutexLocker locker(&mutex);
    return db.tables();
}

QSqlQuery Database::exec(const QString& query) {
    QMutexLocker locker(&mutex);
    QSqlQuery result = db.exec(query);
    return result;
}

void Database::transaction() {
    QMutexLocker locker(&mutex);
    db.transaction();
}

void Database::commit() {
    QMutexLocker locker(&mutex);
    db.commit();
}

Database::Database() {
    qDebug() << "Database::Database()";
    db = QSqlDatabase::addDatabase("QSQLITE", Config::MAINDB_NAME);
    db.setDatabaseName(Config::MUSICLIBDB);
    db.open();
}
