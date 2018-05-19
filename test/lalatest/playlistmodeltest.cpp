#include "playlistmodeltest.h"
#include <QDebug>
#include <QTest>
#include "../../src/playlistmodel.h"
#include "../../src/track.h"

PlaylistModelTest::PlaylistModelTest(QObject* parent) : QObject(parent) {
}

void PlaylistModelTest::testAppend() {
    PlaylistModel plm;
    Track track;
    track.album_ = "lala";
    track.mrl_ = "file:///lalala";
    plm.append(track);
    qDebug() << plm.rowCount();
    QVERIFY(plm.rowCount() == 1);
}
