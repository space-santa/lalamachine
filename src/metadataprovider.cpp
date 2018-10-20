/*
Copyright 2015 Armin Zirkel

This file is part of lalamachine.

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
*/

#include "metadataprovider.h"

#include <memory>

#ifdef _WIN32
#include "WinTag.h"
typedef WinTag TheTag;
#else
#include "TagLibTag.h"
typedef TagLibTag TheTag;
#endif
#include "exceptions.h"
#include "tags.h"

QJsonObject MetaDataProvider::metaData(const QUrl& path) const {
    try {
        Tags tags(std::unique_ptr<TheTag>(new TheTag(path)));
        return tags.toJson();
    } catch (...) {
        throw NoMetaDataException("");
    }
}
