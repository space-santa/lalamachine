#ifndef TESTAUTOPLAYLIST_H
#define TESTAUTOPLAYLIST_H

#include <QObject>

class TestAutoPlaylist : public QObject {
    Q_OBJECT
private slots:
    void initTestCase();
    void testToQuery();
    void cleanupTestCase();
};

#endif  // TESTAUTOPLAYLIST_H
