#pragma once
#include <QString>
#include <QVariant>

class IQueryResult {
public:
    virtual bool next() = 0;
    virtual QVariant value(const QString& val) = 0;
    virtual void first() = 0;
};