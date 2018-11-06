#include <QCoreApplication>
#include <QTest>
#include "QueryBuilder.test.h"
#include "model.test.h"
#include "playlistmodeltest.h"

int main() {
    QueryBuilderTest queryBuilder;
    QTest::qExec(&queryBuilder);

    ModelTest modelTest;
    QTest::qExec(&modelTest);

    PlaylistModelTest playlistModelTest;
    QTest::qExec(&playlistModelTest);

    return 0;
}
