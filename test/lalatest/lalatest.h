#ifndef LALATEST_H
#define LALATEST_H

#include <QObject>
#include <QString>

class LalaTest : public QObject {
    Q_OBJECT
private slots:
    void autoPlaylistObject();
};

#endif  // LALATEST_H
