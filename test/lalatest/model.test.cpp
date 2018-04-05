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

void ModelTest::testGenre() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    auto result = model.genre("rockystuff");
    QCOMPARE(tmpDB->execQueries.first().contains("ROCKYSTUFF"), true);
}

void ModelTest::testUpdateTableNoMusicLib() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    model.updateTable();
    QCOMPARE(tmpDB->execQueries.count(), 1);
    QCOMPARE(tmpDB->execQueries.first().startsWith("CREATE TABLE `musiclib`"), true);
}

void ModelTest::testUpdateTableWithMusicLibWithDateAdded() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    tmpDB->tableList << "musiclib";
    tmpDB->queryRetval = QVariant(QString("dateAdded"));
    model.updateTable();
    QCOMPARE(tmpDB->execQueries.count(), 2);
    QCOMPARE(tmpDB->execQueries.first(), QString("PRAGMA table_info(musiclib)"));
    QCOMPARE(tmpDB->execQueries.at(1), QString("ALTER TABLE musiclib ADD COLUMN discNumber INT NOT NULL DEFAULT 1"));
}

void ModelTest::testUpdateTableWithMusicLibWithDiscNumber() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    tmpDB->tableList << "musiclib";
    tmpDB->queryRetval = QVariant(QString("discNumber"));
    model.updateTable();
    QCOMPARE(tmpDB->execQueries.count(), 2);
    QCOMPARE(tmpDB->execQueries.first(), QString("PRAGMA table_info(musiclib)"));
    QCOMPARE(tmpDB->execQueries.at(1), QString("ALTER TABLE musiclib ADD COLUMN dateAdded TEXT"));
}

void ModelTest::testCopyLibToTmp() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    model.copyLibToTmp();
    QCOMPARE(tmpDB->execQueries.count(), 2);
    QCOMPARE(tmpDB->execQueries.first().startsWith("CREATE TABLE `tmplib`"), true);
    QCOMPARE(tmpDB->execQueries.at(1), QString("insert into tmplib SELECT * from musiclib"));
}

void ModelTest::testClearMusicLib() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    model.clearMusicLib();
    QCOMPARE(tmpDB->execQueries.count(), 1);
    QCOMPARE(tmpDB->execQueries.first(), QString("DELETE FROM musiclib"));
}

void ModelTest::testSetDateAddedForMrl() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    model.setDateAddedForMrl("abc", "def");
    QCOMPARE(tmpDB->execQueries.count(), 1);
    QCOMPARE(tmpDB->execQueries.first(), QString("UPDATE musiclib SET dateAdded='abc' WHERE mrl='def'"));
}

void ModelTest::testGetTablesToRestoreMetaDataNoMusic() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    tmpDB->tableList << "tmplib";
    QVERIFY_EXCEPTION_THROWN(model.getTablesToRestoreMetaData(), TableNotFoundError);
}

void ModelTest::testGetTablesToRestoreMetaDataNoTmp() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    tmpDB->tableList << "musiclib";
    QVERIFY_EXCEPTION_THROWN(model.getTablesToRestoreMetaData(), TableNotFoundError);
}

void ModelTest::testGetTablesToRestoreMetaDataAllGood() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    tmpDB->tableList << "musiclib"
                     << "tmplib";
    QCOMPARE(model.getTablesToRestoreMetaData(), tmpDB->tableList);
}

void ModelTest::testRestoreMetaData() {
    Model model(std::unique_ptr<IMainDB>(new MainDBMock()));
    MainDBMock* tmpDB = dynamic_cast<MainDBMock*>(model.mainDB.get());
    tmpDB->execQueries.clear();
    tmpDB->tableList << "musiclib"
                     << "tmplib";
    model.restoreMetaData();
    QCOMPARE(tmpDB->execQueries.last(), QString("DROP TABLE tmplib"));
}

void ModelTest::testCleanPath() {
    QCOMPARE(Model::cleanPath("file://lalala"), QString("lalala"));
    QCOMPARE(Model::cleanPath("lfile://lalala"), QString("lfile://lalala"));
}
