#include <QCoreApplication>
#include <QTest>
#include "lalatest.h"

int main()
{
    LalaTest lala;
    QTest::qExec(&lala);

    return 0;
}
