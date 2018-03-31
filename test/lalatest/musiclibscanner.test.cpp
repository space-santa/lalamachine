#include "musiclibscanner.test.h"

#include <QSignalSpy>
#include <QtTest>

#include "musiclibscanner.test.mock.h"

void MusicLibScannerTest::initTestCase() {
}

void MusicLibScannerTest::cleanupTestCase() {
}

std::unique_ptr<MusicLibScanner> MusicLibScannerTest::factory(const QStringList& args) {
    auto* rawDirWalker = new DirWalkerMock();
    auto* rawMetaDataProvider = new MetaDataProviderMock();
    auto* rawScannerDb = new ScannerDBMock();

    rawDirWalker->getMusicFileListThrows = args.contains(QString("walkerThrows"));
    rawMetaDataProvider->metaDataThrows = args.contains(QString("metaThrows"));
    rawScannerDb->addQueryThrows = args.contains(QString("addQueryThrows"));
    rawScannerDb->openThrows = args.contains(QString("openThrows"));

    auto dirWalker = std::unique_ptr<IDirWalker>(rawDirWalker);
    auto metaDataProvider = std::unique_ptr<IMetaDataProvider>(rawMetaDataProvider);
    auto scannerDb = std::unique_ptr<IScannerDB>(rawScannerDb);

    auto retval = std::unique_ptr<MusicLibScanner>(
        new MusicLibScanner(std::move(scannerDb), std::move(dirWalker), std::move(metaDataProvider)));

    return retval;
}

void MusicLibScannerTest::testRootPathEmpty() {
    auto scanner = factory(QStringList());
    QSignalSpy startSpy(scanner.get(), SIGNAL(scanStarted()));
    QSignalSpy completeSpy(scanner.get(), SIGNAL(scanComplete()));
    scanner->scanLib("");
    QCOMPARE(startSpy.count(), 0);
    QCOMPARE(completeSpy.count(), 0);
}

void MusicLibScannerTest::testDirWalkerThrows() {
    QStringList args;
    args << "walkerThrows";
    auto scanner = factory(args);
    QSignalSpy startSpy(scanner.get(), SIGNAL(scanStarted()));
    QSignalSpy completeSpy(scanner.get(), SIGNAL(scanComplete()));
    scanner->scanLib("lala");
    QCOMPARE(startSpy.count(), 0);
    QCOMPARE(completeSpy.count(), 0);
}

void MusicLibScannerTest::testOpenThrows() {
    QStringList args;
    args << "openThrows";
    auto scanner = factory(args);
    QSignalSpy startSpy(scanner.get(), SIGNAL(scanStarted()));
    QSignalSpy completeSpy(scanner.get(), SIGNAL(scanComplete()));
    scanner->scanLib("lala");
    QCOMPARE(startSpy.count(), 0);
    QCOMPARE(completeSpy.count(), 0);
}

void MusicLibScannerTest::testNoMetaData() {
    QStringList args;
    args << "metaThrows";
    auto scanner = factory(args);
    QSignalSpy startSpy(scanner.get(), SIGNAL(scanStarted()));
    QSignalSpy completeSpy(scanner.get(), SIGNAL(scanComplete()));
    scanner->scanLib("lala");
    QCOMPARE(startSpy.count(), 1);
    QCOMPARE(completeSpy.count(), 1);
}

void MusicLibScannerTest::testAddQueryThrows() {
    QStringList args;
    args << "addQueryThrows";
    auto scanner = factory(args);
    QSignalSpy startSpy(scanner.get(), SIGNAL(scanStarted()));
    QSignalSpy completeSpy(scanner.get(), SIGNAL(scanComplete()));
    scanner->scanLib("lala");
    QCOMPARE(startSpy.count(), 1);
    QCOMPARE(completeSpy.count(), 1);
}

void MusicLibScannerTest::testSuccess() {
    auto scanner = factory(QStringList());
    QSignalSpy startSpy(scanner.get(), SIGNAL(scanStarted()));
    QSignalSpy completeSpy(scanner.get(), SIGNAL(scanComplete()));
    scanner->scanLib("lala");
    QCOMPARE(startSpy.count(), 1);
    QCOMPARE(completeSpy.count(), 1);
}
