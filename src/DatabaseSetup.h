#pragma once

#include <QString>

class DatabaseSetup {
public:
    DatabaseSetup();
    ~DatabaseSetup();

private:
    void add(const QString& name);
};
