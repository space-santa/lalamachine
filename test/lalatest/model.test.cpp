#include "model.test.h"
#include <QDebug>
#include <QTest>
#include <memory>
#include "../../src/exceptions.h"
#include "../../src/model.h"
#include "model.test.mock.h"

void ModelTest::testResultToList_data() {
    QTest::addColumn<QVariant>("value");
    QTest::addColumn<bool>("hasNext");
    QTest::addColumn<int>("count");
    QTest::addColumn<QString>("result");

    QTest::newRow("good") << QVariant("result") << true << 1 << QString("result");
    QTest::newRow("empty") << QVariant() << true << 0 << QString();
    QTest::newRow("no next") << QVariant("result") << false << 0 << QString();
}

void ModelTest::testResultToList() {
    QFETCH(QVariant, value);
    QFETCH(bool, hasNext);
    QFETCH(int, count);
    QFETCH(QString, result);

    auto* rawResult = new QueryResultMock();
    rawResult->retval = value;
    rawResult->hasNext = !hasNext;
    auto queryResult = std::unique_ptr<IQueryResult>(rawResult);
    auto list = Model::resultToList(queryResult, "lala");
    QCOMPARE(list.count(), count);
    if (count > 0) {
        QCOMPARE(list.first(), result);
    }
}

void ModelTest::testCleanPath() {
    QCOMPARE(Model::cleanPath("file://lalala"), QString("lalala"));
    QCOMPARE(Model::cleanPath("lfile://lalala"), QString("lfile://lalala"));
}
