#include <QCoreApplication>
#include <QTest>
#include "QueryBuilder.test.h"
#include "lalatest.h"
#include "model.test.h"
#include "playlistmodeltest.h"
#include "testautoplaylist.h"

int main() {
    LalaTest lala;
    QTest::qExec(&lala);

    TestAutoPlaylist test2;
    QTest::qExec(&test2);

    QueryBuilderTest queryBuilder;
    QTest::qExec(&queryBuilder);

    ModelTest modelTest;
    QTest::qExec(&modelTest);

    PlaylistModelTest playlistModelTest;
    QTest::qExec(&playlistModelTest);

    return 0;
}
