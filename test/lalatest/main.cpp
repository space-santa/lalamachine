#include <QCoreApplication>
#include <QTest>
#include "lalatest.h"
#include "test_model.h"
#include "testautoplaylist.h"

int main() {
  LalaTest lala;
  // QTest::qExec(&lala);

  TestAutoPlaylist test2;
  // QTest::qExec(&test2);

  ModelTest test_model;
  QTest::qExec(&test_model);

  return 0;
}
