#include <QCoreApplication>
#include <QTest>
#include "QueryBuilder.test.h"
#include "lalatest.h"
#include "maindbtest.h"
#include "model.test.h"
#include "testautoplaylist.h"

int main() {
    LalaTest lala;
    // QTest::qExec(&lala);

    TestAutoPlaylist test2;
    // QTest::qExec(&test2);

    QueryBuilderTest queryBuilder;
    QTest::qExec(&queryBuilder);

    ModelTest modelTest;
    QTest::qExec(&modelTest);

    MainDBTest mainDBTest;
    QTest::qExec(&mainDBTest);

    return 0;
}
