#pragma once

#include "tags.h"

class IMetaDataProvider {
public:
    virtual Tags metaData(const QUrl& path) const = 0;
};