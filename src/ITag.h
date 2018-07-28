#pragma once

#include <QString>

class ITag {
public:
    virtual QString album() = 0;
    virtual QString artist() = 0;
    virtual QString comment() = 0;
    virtual QString genre() = 0;
    virtual QString title() = 0;
    virtual QString track() = 0;
    virtual QString year() = 0;
    virtual QString discNumber() = 0;
    virtual QString length() = 0;
    virtual QString lengthString() = 0;
    virtual QString path() = 0;
};