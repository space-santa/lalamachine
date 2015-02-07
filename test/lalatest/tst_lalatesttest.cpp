#include <QString>
#include <QtTest>
#include <QCoreApplication>

class LalatestTest : public QObject
{
    Q_OBJECT

public:
    LalatestTest();

private Q_SLOTS:
    void initTestCase();
    void cleanupTestCase();
    void testCase1();
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

QTEST_MAIN(LalatestTest)

#include "tst_lalatesttest.moc"
