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
    LalaTypes::AndOr andor_ {};
    LalaTypes::Tag tag_ {};
    LalaTypes::Operator op_ {};
    QString val_ {};
};

#endif // AUTOPLAYLISTOBJECT_H
