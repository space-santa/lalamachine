#pragma once

#include <QSqlDatabase>
#include <QString>
#include "IScannerDB.h"

class QSqlDatabase;

class ScannerDB : public IScannerDB {
public:
    ScannerDB();

    void open();
    void transaction();
    void addQuery(const Tags& tags);
    void commit();
    void close();

private:
    QSqlDatabase db;
    QString getTrackQuery(const Tags& track, const QString& date);
};