#include "lalatest.h"

#include <QFile>
#include <QString>
#include <QStringList>
#include <QtTest>

#include "../../src/autoplaylistobject.h"
#include "../../src/lalatypes.h"
#include "../../src/playlistmodel.h"

void LalaTest::autoPlaylistObject() {
    LalaTypes::AndOr andor = LalaTypes::AND;
    LalaTypes::Tag tag = LalaTypes::GENRE;
    LalaTypes::Operator op = LalaTypes::CONTAINS;
    QString val = "House";
    AutoPlaylistObject apo(andor, tag, op, val);
    Q_ASSERT(apo.andor() == andor);
    Q_ASSERT(apo.tag() == tag);
    Q_ASSERT(apo.op() == op);
    Q_ASSERT(apo.val() == val);

    QJsonObject obj = apo.toJson();

    AutoPlaylistObject apo2(obj);
    Q_ASSERT(apo.andor() == apo2.andor());
    Q_ASSERT(apo.tag() == apo2.tag());
    Q_ASSERT(apo.op() == apo2.op());
    Q_ASSERT(apo.val() == apo2.val());
}
