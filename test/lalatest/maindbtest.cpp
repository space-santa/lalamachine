#include "maindbtest.h"
#include <QTest>
#include "../../src/MainDB.h"

void MainDBTest::testExec()
{
    MainDB db;
    QString query("PRAGMA table_info(musiclib)");
    db.exec(query);
}
