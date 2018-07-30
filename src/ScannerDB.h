#pragma once

#include <QString>
#include <QStringList>
#include "IScannerDB.h"

class ScannerDB : public IScannerDB {
public:
    void init();
    void kill();
    void open();
    void transaction();
    void addQuery(const QJsonObject& tags);
    void commit();
    void close();

private:
    QString getTrackQuery(const QJsonObject& track, const QString& date);
    QStringList queryList;
    void clearMusicLib();
};
