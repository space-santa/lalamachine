#include "testautoplaylist.h"

#include <QTest>
#include <QDebug>

#include "../../autoplaylist.h"
#include "../../autoplaylistobject.h"
#include "../../lalatypes.h"
#include "../../musiclib.h"

void TestAutoPlaylist::initTestCase()
{
    //
}

void TestAutoPlaylist::testToQuery()
{
    AutoPlaylist list("lala");
    AutoPlaylistObject apo1(LalaTypes::AND,
                            LalaTypes::GENRE,
                            LalaTypes::IS,
                            "Electro House");
    AutoPlaylistObject apo2(LalaTypes::OR,
                            LalaTypes::ALBUM,
                            LalaTypes::IS_NOT,
                            "stuff");
    AutoPlaylistObject apo3(LalaTypes::AND,
                            LalaTypes::ARTIST,
                            LalaTypes::CONTAINS,
                            "moo");
    list.addApo(apo1);
    list.addApo(apo2);
    list.addApo(apo3);

    QString query(
        "SELECT * FROM musiclib WHERE genre IS 'Electro House' OR album IS NOT "
        "'stuff' AND artist LIKE '%moo%' ORDER BY artist, album, track");
    qDebug() << list.toQuery();
    QVERIFY(list.toQuery() == query);
}

void TestAutoPlaylist::cleanupTestCase()
{
    //
}
