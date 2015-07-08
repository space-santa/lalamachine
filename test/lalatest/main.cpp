#include <QCoreApplication>
#include <QTest>
#include "lalatest.h"
#include "testautoplaylist.h"

int main()
{
    LalaTest lala;
    QTest::qExec(&lala);

    TestAutoPlaylist test2;
    QTest::qExec(&test2);

    return 0;
}
