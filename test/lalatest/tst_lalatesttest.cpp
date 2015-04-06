#include <QString>
#include <QtTest>
#include <QCoreApplication>
#include <QStringList>

#include "../../timeconverter.h"
#include "../../systeminteractor.h"
#include "../../m3uinout.h"
#include "../../config.h"
#include "../../autoplaylistobject.h"
#include "../../lalatypes.h"

class LalatestTest : public QObject
{
    Q_OBJECT

public:
    LalatestTest();

    QString backupFileName(QString path);
    void backupFile(QString path);
    void restoreBackup(QString path);

private Q_SLOTS:
    void initTestCase();
    void cleanupTestCase();
    void testCase1();
    void timeTest();
    void systemInteractor();
    void config();
    //void m3uInOut();
    void autoPlaylistObject();
};

LalatestTest::LalatestTest()
{
}

QString LalatestTest::backupFileName(QString path)
{
    return path + ".backup";
}

void LalatestTest::backupFile(QString path)
{
    if (QFile::exists(backupFileName(path))) {
        QFile::remove(backupFileName(path));
    }
    QFile::copy(path, backupFileName(path));
}

void LalatestTest::restoreBackup(QString path)
{
    if (QFile::exists(path)) {
        QFile::remove(path);
    }
    QFile::rename(backupFileName(path), path);
}

void LalatestTest::initTestCase()
{
}

void LalatestTest::cleanupTestCase()
{
}

void LalatestTest::testCase1()
{
    QVERIFY2(true, "Failure");
}

void LalatestTest::timeTest()
{
    QString testString("");
    TimeConverter tc;
    // Initialization
    testString = "00:00";
    QVERIFY(tc.toString() == testString);
    // one minute
    testString = "01:00";
    tc.setSeconds(60);
    QVERIFY(tc.toString() == testString);
    // one hour
    testString = "01:00:00";
    tc.setSeconds(3600);
    QVERIFY(tc.toString() == testString);
    // one day
    testString = "1 day, 00:00:00";
    tc.setSeconds(24 * 3600);
    QVERIFY(tc.toString() == testString);
    // one day, one minute and 23 seconds
    testString = "1 day, 00:01:23";
    tc.setSeconds(24 * 3600 + 60 + 23);
    QVERIFY(tc.toString() == testString);
    // 12 days, 22 hours, thirtyone minutes and 23 seconds
    testString = "12 days, 22:31:23";
    tc.setSeconds(12 * 24 * 3600 + 22 * 3600 + 31 * 60 + 23);
    QVERIFY(tc.toString() == testString);
}

void LalatestTest::systemInteractor()
{
    SystemInteractor sysInt;
    // That'll work as expected.
    QVERIFY(sysInt.exec("echo hello") == QString("hello"));
    // Since gugulala25341 doesn't exist it will return an empty string.
    QVERIFY(sysInt.exec("gugulala25341") == QString(""));
    // This will return true since echo can successfully be started detached.
    QVERIFY(sysInt.startDetached(QString("echo"), QStringList()));
    // Now it will be false since we can't successfully run nonexisting stuff.
    QVERIFY(!sysInt.startDetached(QString("gugulala25341"), QStringList()));
}

void LalatestTest::config()
{
    backupFile(Config::CONFIGPATH);
    Config cfg;
    double oldvol = cfg.volume();
    double newvol = 0.37;
    cfg.setVolume(newvol);
    cfg.saveConfig();
    Config cfg2;
    QVERIFY(cfg2.volume() == newvol);

    // Testing a path.
    // setLibPath automatically saves the json file.
    cfg.setLibPath("/tmp");
    cfg2.loadConfig();
    QVERIFY(cfg2.libPath() == cfg.libPath());
    // A QUrl must also work.
    cfg.setLibPath("file:///tmp");
    cfg2.loadConfig();
    QVERIFY(cfg2.libPath() == cfg.libPath());

    restoreBackup(Config::CONFIGPATH);
    // This is to check if the restoreBackup worked.
    Config cfg3;
    QVERIFY(cfg3.volume() == oldvol);
}

//void LalatestTest::m3uInOut()
//{
//    M3uInOut m3;
//    QString laladir(Config::PLAYLISTDIR);
//    QString name("testlist456723");
//    QStringList testlist(QStringList() << "lala.mp3"
//                         << "gugu.mp3" << "dada.mp3"
//                         << "pupu.mp3");
//    QStringList returnList;

//    for (int i = 0; i < testlist.length(); ++i) {
//        returnList << "file://" + testlist[i];
//    }

//    QVERIFY(m3.m3uPath(name) == QString(laladir + "/" + name + ".m3u"));
//    m3.writePlaylist(name, testlist);
//    QVERIFY(m3.getPlaylistNames().contains(name));
//    QVERIFY(m3.readPlaylist(name) == returnList);
//    m3.deletePlaylist(name);
//    QVERIFY(!m3.getPlaylistNames().contains(name));
//    // A nonexisting playlistname will return an empty list.
//    QVERIFY(m3.readPlaylist("strangeunlikelyplaylistnamethatdoesntexist")
//            == QStringList());
//}

void LalatestTest::autoPlaylistObject()
{
    LalaTypes::Tag tag = LalaTypes::GENRE;
    LalaTypes::Operator op = LalaTypes::CONTAINS;
    QString val = "House";
    AutoPlaylistObject apo(tag, op, val);
    Q_ASSERT(apo.tag() == tag);
    Q_ASSERT(apo.op() == op);
    Q_ASSERT(apo.val() == val);

    QJsonObject obj = apo.toJson();

    AutoPlaylistObject apo2(obj);
    Q_ASSERT(apo.tag() == apo2.tag());
    Q_ASSERT(apo.op() == apo2.op());
    Q_ASSERT(apo.val() == apo2.val());

}

QTEST_MAIN(LalatestTest)

#include "tst_lalatesttest.moc"
