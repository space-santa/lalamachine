#ifndef LALATEST_H
#define LALATEST_H

#include <QObject>
#include <QString>

class LalaTest : public QObject {
    Q_OBJECT
private slots:
    void initTestCase();
    void cleanupTestCase();
    void testCase1();
    void timeTest();
    void config();
    // void m3uInOut();
    void autoPlaylistObject();

private:
    QString backupFileName(QString path);
    void backupFile(QString path);
    void restoreBackup(QString path);
};

#endif  // LALATEST_H
