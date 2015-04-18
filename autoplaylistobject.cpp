#include "autoplaylistobject.h"

#include <QJsonObject>
#include <QMap>
#include "lalatypes.h"

AutoPlaylistObject::AutoPlaylistObject(const QJsonObject &obj)
{
    fromJson(obj);
}

AutoPlaylistObject::AutoPlaylistObject(LalaTypes::AndOr andor,
                                       LalaTypes::Tag tag,
                                       LalaTypes::Operator op,
                                       QString val)
    : andor_(andor), tag_(tag), op_(op), val_(val)
{
}

LalaTypes::AndOr AutoPlaylistObject::andor() const
{
    return andor_;
}

LalaTypes::Tag AutoPlaylistObject::tag() const
{
    return tag_;
}

LalaTypes::Operator AutoPlaylistObject::op() const
{
    return op_;
}

QString AutoPlaylistObject::val() const
{
    return val_;
}

QJsonObject AutoPlaylistObject::toJson() const
{
    QJsonObject retval;
    retval.insert("andor", LalaTypes::ANDOR_MAP.value(andor_));
    retval.insert("tag", LalaTypes::TAG_MAP.value(tag_));
    retval.insert("operator", LalaTypes::OP_MAP.value(op_));
    retval.insert("value", val_);

    return retval;
}

void AutoPlaylistObject::fromJson(const QJsonObject &obj)
{
    andor_ = LalaTypes::ANDOR_MAP.key(obj.value("andor").toString());
    tag_ = LalaTypes::TAG_MAP.key(obj.value("tag").toString());
    op_ = LalaTypes::OP_MAP.key(obj.value("operator").toString());
    val_ = obj.value("value").toString();
}
