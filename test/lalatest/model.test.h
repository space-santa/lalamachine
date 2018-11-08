#pragma once

#include <QObject>

class ModelTest : public QObject {
    Q_OBJECT
private slots:
    void testResultToList_data();
    void testResultToList();
    void testCleanPath();
};
