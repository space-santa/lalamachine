#include <QString>
#include <QtTest>
#include <QCoreApplication>

#include "../../timeconverter.h"

class LalatestTest : public QObject
{
    Q_OBJECT

public:
    LalatestTest();

private Q_SLOTS:
    void initTestCase();
    void cleanupTestCase();
    void testCase1();
    void timeTest();
};

LalatestTest::LalatestTest()
{
}

void LalatestTest::initTestCase()
{
}

void LalatestTest::cleanupTestCase()
{
}

void LalatestTest::testCase1()
{
    QVERIFY2(true, "Failure");
}

void LalatestTest::timeTest() {
    QString testString("");
    TimeConverter tc;
    // one minute
    testString = "01:00";
    tc.setSeconds(60);
    Q_ASSERT(tc.toString() == testString);
    // one hour
    testString = "01:00:00";
    tc.setSeconds(3600);
    Q_ASSERT(tc.toString() == testString);
    // one day
    testString = "1:00:00:00";
    tc.setSeconds(24 * 3600);
    Q_ASSERT(tc.toString() == testString);
    // one day, one minute and 23 seconds
    testString = "1:00:01:23";
    tc.setSeconds(24 * 3600 + 60 + 23);
    Q_ASSERT(tc.toString() == testString);
    // 12 days, 22 hours, thirtyone minutes and 23 seconds
    testString = "12:22:31:23";
    tc.setSeconds(12 *24 * 3600 + 22 * 3600 + 31 * 60 + 23);
    Q_ASSERT(tc.toString() == testString);
}

QTEST_MAIN(LalatestTest)

#include "tst_lalatesttest.moc"
