#pragma once

#include <QJsonObject>
class IScannerDB {
public:
    virtual ~IScannerDB(){};
    virtual void open() = 0;
    virtual void transaction() = 0;
    virtual void addQuery(const QJsonObject& tags) = 0;
    virtual void commit() = 0;
    virtual void close() = 0;
};