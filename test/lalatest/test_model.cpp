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

void ModelTest::testGenreQuery() {
  QString query("SELECT DISTINCT genre FROM musiclib ORDER BY genre ASC");
  QString filteredQuery(
      "SELECT DISTINCT genre FROM musiclib WHERE UPPER(genre) LIKE '%METAL%' "
      "ORDER BY genre ASC");

  QVERIFY(Model::genreQuery() == query);
  QVERIFY(Model::genreQuery(QString("Metal")) == filteredQuery);
}

void ModelTest::testArtistQuery() {
  QString query(
      "SELECT DISTINCT artist FROM musiclib WHERE artist NOT NULL ORDER BY "
      "artist ASC");
  QVERIFY(Model::artistQuery() == query);

  QString query_artist(
      "SELECT DISTINCT artist FROM musiclib WHERE artist NOT NULL AND "
      "UPPER(artist) LIKE '%LALA%' ORDER BY artist ASC");
  QVERIFY(Model::artistQuery("lala") == query_artist);

  QString query_genre(
      "SELECT DISTINCT artist FROM musiclib WHERE artist NOT NULL AND "
      "genre = 'lala' ORDER BY artist ASC");
  QVERIFY(Model::artistQuery("", "lala") == query_genre);

  // Only consider the genre if the artist is empty. That means this returns
  // the `query_artist`.
  QVERIFY(Model::artistQuery("lala", "bubu") == query_artist);
}

void ModelTest::testAlbumQuery() {
  QString query(
      "SELECT DISTINCT album FROM musiclib WHERE album NOT NULL ORDER BY album "
      "ASC");
  QVERIFY(Model::albumQuery() == query);

  QString query_album(
      "SELECT DISTINCT album FROM musiclib WHERE album NOT NULL AND "
      "UPPER(album) LIKE '%LALA%' ORDER BY album ASC");
  QVERIFY(Model::albumQuery("lala") == query_album);
  QVERIFY(Model::albumQuery("lala", "bubu") == query_album);
  QVERIFY(Model::albumQuery("lala", "", "bubu") == query_album);
  QVERIFY(Model::albumQuery("lala", "dada", "bubu") == query_album);

  QString query_artist(
      "SELECT DISTINCT album FROM musiclib WHERE album NOT NULL AND "
      "artist = 'dada' ORDER BY album ASC");
  QVERIFY(Model::albumQuery("", "dada") == query_artist);

  QString query_genre(
      "SELECT DISTINCT album FROM musiclib WHERE album NOT NULL AND "
      "genre = 'bubu' ORDER BY album ASC");
  QVERIFY(Model::albumQuery("", "", "bubu") == query_genre);

  QString query_genre_artist(
      "SELECT DISTINCT album FROM musiclib WHERE album NOT NULL AND "
      "genre = 'bubu' AND artist = 'dada' ORDER BY album ASC");
  QVERIFY(Model::albumQuery("", "dada", "bubu") == query_genre_artist);
}