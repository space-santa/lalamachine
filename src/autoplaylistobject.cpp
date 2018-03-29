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

#include "autoplaylistobject.h"

#include <QJsonObject>
#include <QMap>
#include "lalatypes.h"
#include "model.h"
#include "musiclib.h"

AutoPlaylistObject::AutoPlaylistObject(const QJsonObject& obj) {
    fromJson(obj);
}

AutoPlaylistObject::AutoPlaylistObject(LalaTypes::AndOr andor, LalaTypes::Tag tag, LalaTypes::Operator op, QString val)
    : andor_(andor), tag_(tag), op_(op), val_(val) {}

LalaTypes::AndOr AutoPlaylistObject::andor() const {
    return andor_;
}

LalaTypes::Tag AutoPlaylistObject::tag() const {
    return tag_;
}

LalaTypes::Operator AutoPlaylistObject::op() const {
    return op_;
}

QString AutoPlaylistObject::val() const {
    return val_;
}

QJsonObject AutoPlaylistObject::toJson() const {
    QJsonObject retval;
    retval.insert("andor", LalaTypes::ANDOR_MAP.value(andor_));
    retval.insert("tag", LalaTypes::TAG_MAP.value(tag_));
    retval.insert("operator", LalaTypes::OP_MAP.value(op_));
    retval.insert("value", val_);

    return retval;
}

void AutoPlaylistObject::fromJson(const QJsonObject& obj) {
    andor_ = LalaTypes::ANDOR_MAP.key(obj.value("andor").toString().toUpper());
    tag_ = LalaTypes::TAG_MAP.key(obj.value("tag").toString().toLower());
    op_ = LalaTypes::OP_MAP.key(obj.value("operator").toString().toUpper());
    val_ = obj.value("value").toString();
}

QString AutoPlaylistObject::toQuery(bool isFirst) const {
    QString query(" ");
    if (!isFirst) {
        query.append(LalaTypes::ANDOR_MAP.value(andor()) + " ");
    }
    query.append(LalaTypes::TAG_MAP.value(tag()) + " ");
    query.append(LalaTypes::OP_MAP.value(op()) + " ");
    QString tmp("");

    if (op() == LalaTypes::CONTAINS || op() == LalaTypes::CONTAINS_NOT) {
        tmp = " '%%1%'";
    } else {
        tmp = " '%1'";
    }
    query.append(tmp.arg(Model::escapeString(val())));

    return query;
}
