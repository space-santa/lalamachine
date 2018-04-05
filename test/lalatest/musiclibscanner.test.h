#pragma once

#include <QObject>
#include <memory>

#include "../../src/musiclibscanner.h"

class MusicLibScannerTest : public QObject {
    Q_OBJECT
private slots:
    void initTestCase();
    void cleanupTestCase();
    void testRootPathEmpty();
    void testDirWalkerThrows();
    void testOpenThrows();
    void testNoMetaData();
    void testAddQueryThrows();
    void testSuccess();

private:
    std::unique_ptr<MusicLibScanner> factory(const QStringList& args);
};
