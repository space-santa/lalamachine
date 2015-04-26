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

#ifndef AUTOPLAYLISTOBJECT_H
#define AUTOPLAYLISTOBJECT_H

#include <QString>
#include <QJsonObject>
#include "lalatypes.h"

class AutoPlaylistObject
{
public:
    AutoPlaylistObject(const QJsonObject &obj);
    AutoPlaylistObject(LalaTypes::AndOr andor,
                       LalaTypes::Tag tag,
                       LalaTypes::Operator op,
                       QString val);

    LalaTypes::AndOr andor() const;
    LalaTypes::Tag tag() const;
    LalaTypes::Operator op() const;
    QString val() const;

    QJsonObject toJson() const;
    void fromJson(const QJsonObject &obj);

private:
    LalaTypes::AndOr andor_{};
    LalaTypes::Tag tag_{};
    LalaTypes::Operator op_{};
    QString val_{};
};

#endif  // AUTOPLAYLISTOBJECT_H
