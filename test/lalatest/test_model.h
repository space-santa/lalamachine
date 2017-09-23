#pragma once

#include <QObject>

class ModelTest : public QObject {
  Q_OBJECT
 private slots:
  void initTestCase();
  void cleanupTestCase();
  void testCase1();

  void escapeString_data();
  void escapeString();

  void testGenreQuery();
  void testArtistQuery();
};
