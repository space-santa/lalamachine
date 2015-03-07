#include <QString>
#include <QtTest>
#include <QCoreApplication>
#include <QStringList>

#include "../../timeconverter.h"
#include "../../systeminteractor.h"

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
    void systemInteractor();
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
    testString = "1:00:00:00";
    tc.setSeconds(24 * 3600);
    QVERIFY(tc.toString() == testString);
    // one day, one minute and 23 seconds
    testString = "1:00:01:23";
    tc.setSeconds(24 * 3600 + 60 + 23);
    QVERIFY(tc.toString() == testString);
    // 12 days, 22 hours, thirtyone minutes and 23 seconds
    testString = "12:22:31:23";
    tc.setSeconds(12 *24 * 3600 + 22 * 3600 + 31 * 60 + 23);
    QVERIFY(tc.toString() == testString);
}

void LalatestTest::systemInteractor()
{
    SystemInteractor sysInt;
    // That'll work as expected.
    QVERIFY(sysInt.exec("echo hello") == QString("hello"));
    // Since gugulala25341 doesn't exist it will return an empty string.
    QVERIFY(sysInt.exec("gugulala25341") == QString(""));
    // This will return true since echo can successfully be started detached.
    QVERIFY(sysInt.startDetached(QString("echo"), QStringList()));
    // Now it will be false since we can't successfully run nonexisting stuff.
    QVERIFY(!sysInt.startDetached(QString("gugulala25341"), QStringList()));
}

QTEST_MAIN(LalatestTest)

#include "tst_lalatesttest.moc"
