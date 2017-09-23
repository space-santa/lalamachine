#pragma once

#include <QJsonArray>
#include <QJsonObject>
#include <QMutex>
#include <QSharedPointer>
#include <QSqlDatabase>
#include <QSqlQuery>
#include <QSqlResult>
#include <QString>
#include <QStringList>

class Model {
  friend class ModelTest;

 public:
  Model();

  QStringList genre(const QString &filter);
  QStringList artist(const QString &filter, const QString &genre = QString());
  QStringList album(const QString &filter, const QString &genre = QString(),
                    const QString &artist = QString());

  QJsonObject trackDetails(const QString &mrl) const;

  static QString escapeString(QString string);
  static QString genreQuery(const QString &filter = QString());
  static QString artistQuery(const QString &artist = QString(),
                             const QString &genre = QString());
  static QString albumQuery(const QString &album = QString(),
                            const QString &artist = QString(),
                            const QString &genre = QString());

 private:
  QSqlDatabase db_;
  QSharedPointer<QMutex> mutex_;

  void init();
  static QStringList resultToList(QSqlQuery result, const QString &what);
  void updateTable();
  void newUpdateTable();
  void createLibTable(const QString &name);
  void ensureAllTables();
};
