#include "QueryBuilder.test.h"

#include <QString>
#include <QtTest>

#include "../../src/QueryBuilder.h"

void QueryBuilderTest::initTestCase() {
}

void QueryBuilderTest::cleanupTestCase() {
}

void QueryBuilderTest::escapeString_data() {
    QTest::addColumn<QString>("string");
    QTest::addColumn<QString>("result");

    QTest::newRow("O'Rourke") << QString("O'Rourke") << QString("O''Rourke");
    QTest::newRow("O''Rourke") << QString("O''Rourke") << QString("O''''Rourke");
}

void QueryBuilderTest::escapeString() {
    QFETCH(QString, string);
    QFETCH(QString, result);

    QCOMPARE(QueryBuilder::escapeString(string), result);
}

void QueryBuilderTest::testGenreQuery() {
    QString query("SELECT DISTINCT genre FROM musiclib ORDER BY genre ASC");
    QString filteredQuery("SELECT DISTINCT genre FROM musiclib WHERE UPPER(genre) LIKE '%METAL%' "
                          "ORDER BY genre ASC");

    QVERIFY(QueryBuilder::genreQuery() == query);
    QVERIFY(QueryBuilder::genreQuery(QString("Metal")) == filteredQuery);
}

void QueryBuilderTest::testArtistQuery() {
    QString query("SELECT DISTINCT artist FROM musiclib WHERE artist NOT NULL ORDER BY "
                  "artist ASC");
    QVERIFY(QueryBuilder::artistQuery() == query);

    QString query_artist("SELECT DISTINCT artist FROM musiclib WHERE artist NOT NULL AND "
                         "UPPER(artist) LIKE '%LALA%' ORDER BY artist ASC");
    QVERIFY(QueryBuilder::artistQuery("lala") == query_artist);

    QString query_genre("SELECT DISTINCT artist FROM musiclib WHERE artist NOT NULL AND "
                        "genre = 'lala' ORDER BY artist ASC");
    QVERIFY(QueryBuilder::artistQuery("", "lala") == query_genre);

    // Only consider the genre if the artist is empty. That means this returns
    // the `query_artist`.
    QVERIFY(QueryBuilder::artistQuery("lala", "bubu") == query_artist);
}

void QueryBuilderTest::testAlbumQuery() {
    QString query("SELECT DISTINCT album FROM musiclib WHERE album NOT NULL ORDER BY album "
                  "ASC");
    QVERIFY(QueryBuilder::albumQuery() == query);

    QString query_album("SELECT DISTINCT album FROM musiclib WHERE album NOT NULL AND "
                        "UPPER(album) LIKE '%LALA%' ORDER BY album ASC");
    QVERIFY(QueryBuilder::albumQuery("lala") == query_album);
    QVERIFY(QueryBuilder::albumQuery("lala", "bubu") == query_album);
    QVERIFY(QueryBuilder::albumQuery("lala", "", "bubu") == query_album);
    QVERIFY(QueryBuilder::albumQuery("lala", "dada", "bubu") == query_album);

    QString query_artist("SELECT DISTINCT album FROM musiclib WHERE album NOT NULL AND "
                         "artist = 'dada' ORDER BY album ASC");
    QVERIFY(QueryBuilder::albumQuery("", "dada") == query_artist);

    QString query_genre("SELECT DISTINCT album FROM musiclib WHERE album NOT NULL AND "
                        "genre = 'bubu' ORDER BY album ASC");
    QVERIFY(QueryBuilder::albumQuery("", "", "bubu") == query_genre);

    QString query_genre_artist("SELECT DISTINCT album FROM musiclib WHERE album NOT NULL AND "
                               "genre = 'bubu' AND artist = 'dada' ORDER BY album ASC");
    QVERIFY(QueryBuilder::albumQuery("", "dada", "bubu") == query_genre_artist);
}