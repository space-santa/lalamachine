#include "test_model.h"

#include <QString>
#include <QtTest>

#include "../../src/model.h"

void ModelTest::initTestCase() {}

void ModelTest::cleanupTestCase() {}

void ModelTest::testCase1() { QVERIFY2(true, "Failure"); }

void ModelTest::escapeString_data() {
  QTest::addColumn<QString>("string");
  QTest::addColumn<QString>("result");

  QTest::newRow("O'Rourke") << QString("O'Rourke") << QString("O''Rourke");
  QTest::newRow("O''Rourke") << QString("O''Rourke") << QString("O''''Rourke");
}

void ModelTest::escapeString() {
  QFETCH(QString, string);
  QFETCH(QString, result);

  QCOMPARE(Model::escapeString(string), result);
}

void ModelTest::genreQuery() {
  QString query("SELECT DISTINCT genre FROM musiclib ORDER BY genre ASC");
  QString filteredQuery(
      "SELECT DISTINCT genre FROM musiclib WHERE UPPER(genre) LIKE '%METAL%' "
      "ORDER BY genre ASC");

  QVERIFY(Model::genreQuery() == query);
  QVERIFY(Model::genreQuery(QString("Metal")) == filteredQuery);
}