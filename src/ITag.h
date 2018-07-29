#pragma once

#include <QString>

class ITag {
public:
    virtual ~ITag(){};
    virtual QString album() const = 0;
    virtual QString artist() const = 0;
    virtual QString comment() const = 0;
    virtual QString genre() const = 0;
    virtual QString title() const = 0;
    virtual QString track() const = 0;
    virtual QString year() const = 0;
    virtual QString discNumber() const = 0;
    virtual QString length() const = 0;
    virtual QString lengthString() const = 0;
    virtual QString path() const = 0;
};