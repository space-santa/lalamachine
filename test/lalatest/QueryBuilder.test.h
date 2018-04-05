#pragma once

#include <QObject>

class QueryBuilderTest : public QObject {
    Q_OBJECT
private slots:
    void initTestCase();
    void cleanupTestCase();

    void escapeString_data();
    void escapeString();

    void testGenreQuery();
    void testArtistQuery();
    void testAlbumQuery();
};
