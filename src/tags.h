#pragma once

#include <QJsonObject>

#include <memory>

#include "ITag.h"

class Tags {
public:
    Tags(std::unique_ptr<ITag> tag);

    QJsonObject toJson();

    bool isValid();

private:
    std::unique_ptr<ITag> tag_;
};
