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

#include "tags.h"
#include "exceptions.h"
#include "TagLibTag.h"

Tags MetaDataProvider::metaData(const QUrl& path) const {
    std::unique_ptr<ITag> tag = std::unique_ptr<ITag>(new TagLibTag(path));
    try {
        Tags tags(std::move(tag));
        return tags;
    } catch (...) {
        throw NoMetaDataException("");
	}
}
