#include <QCoreApplication>
#include <QTest>
#include "QueryBuilder.test.h"
#include "lalatest.h"
#include "musiclibscanner.test.h"
#include "testautoplaylist.h"

int main() {
    LalaTest lala;
    // QTest::qExec(&lala);

    TestAutoPlaylist test2;
    // QTest::qExec(&test2);

    QueryBuilderTest queryBuilder;
    QTest::qExec(&queryBuilder);

    MusicLibScannerTest scanner;
    QTest::qExec(&scanner);

    return 0;
}
