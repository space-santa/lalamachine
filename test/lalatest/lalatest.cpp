#include "lalatest.h"

#include <QFile>
#include <QString>
#include <QStringList>
#include <QtTest>

#include "../../src/autoplaylistobject.h"
#include "../../src/lalatypes.h"
#include "../../src/playlistmodel.h"
#include "../../src/timeconverter.h"

void LalaTest::timeTest() {
    QString testString("");
    TimeConverter tc;
    // Initialization
    testString = "00:00";
    QVERIFY(tc.toString() == testString);
    // one minute
    testString = "01:00";
    tc.setSeconds(60);
    QVERIFY(tc.toString() == testString);
    // one hour
    testString = "01:00:00";
    tc.setSeconds(3600);
    QVERIFY(tc.toString() == testString);
    // one day
    testString = "1 day, 00:00:00";
    tc.setSeconds(24 * 3600);
    QVERIFY(tc.toString() == testString);
    // one day, one minute and 23 seconds
    testString = "1 day, 00:01:23";
    tc.setSeconds(24 * 3600 + 60 + 23);
    QVERIFY(tc.toString() == testString);
    // 12 days, 22 hours, thirtyone minutes and 23 seconds
    testString = "12 days, 22:31:23";
    tc.setSeconds(12 * 24 * 3600 + 22 * 3600 + 31 * 60 + 23);
    QVERIFY(tc.toString() == testString);
}

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
