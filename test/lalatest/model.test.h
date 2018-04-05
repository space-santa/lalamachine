#pragma once

#include <QObject>

class ModelTest : public QObject {
    Q_OBJECT
private slots:
    void testResultToList_data();
    void testResultToList();
    void testGenre();
    void testUpdateTableNoMusicLib();
    void testUpdateTableWithMusicLibWithDateAdded();
    void testUpdateTableWithMusicLibWithDiscNumber();
    void testCopyLibToTmp();
    void testClearMusicLib();
    void testSetDateAddedForMrl();
    void testGetTablesToRestoreMetaDataNoMusic();
    void testGetTablesToRestoreMetaDataNoTmp();
    void testGetTablesToRestoreMetaDataAllGood();
    void testRestoreMetaData();
    void testCleanPath();
};