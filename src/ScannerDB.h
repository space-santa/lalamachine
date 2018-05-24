#pragma once

#include <QString>
#include "IScannerDB.h"

class ScannerDB : public IScannerDB {
public:
    void open();
    void transaction();
    void addQuery(const Tags& tags);
    void commit();
    void close();

private:
    QString getTrackQuery(const Tags& track, const QString& date);
    QStringList queryList;
};
