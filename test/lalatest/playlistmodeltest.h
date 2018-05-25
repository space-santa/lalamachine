#pragma once

#include <QObject>

class PlaylistModelTest : public QObject {
    Q_OBJECT
public:
    explicit PlaylistModelTest(QObject* parent = nullptr);

signals:

private slots:
    void testAppend();
};
