#pragma once

#include <QSqlDatabase>
#include <QSqlResult>
#include <QSqlQuery>
#include <QString>
#include <QStringList>
#include <QJsonObject>
#include <QJsonArray>

class Model {
 public:
  Model();

  QStringList genre(const QString &filter);
  QStringList artist(const QString &filter, const QString &genre = QString());
  QStringList album(const QString &filter, const QString &genre = QString(),
                    const QString &artist = QString());

  QJsonObject trackDetails(const QString &mrl) const;

 private:
  QSqlDatabase db_;
  void init();
  static QString genreQuery(const QString &filter);
  static QString escapeString(QString string);
  static QStringList resultToList(QSqlQuery result, const QString &what);
};
