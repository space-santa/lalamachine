#pragma once
#include "tags.h"

class IScannerDB {
public:
    virtual void open() = 0;
    virtual void transaction() = 0;
    virtual void addQuery(const Tags& tags) = 0;
    virtual void commit() = 0;
    virtual void close() = 0;
};