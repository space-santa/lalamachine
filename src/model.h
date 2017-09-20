#pragma once

#include <QSqlDatabase>
#include <QString>
#include <QStringList>

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
  void Model::init();
  static QString genreQuery(const QString &filter);
  static QString escapeString(QString &string);
  static QStringList Model::resultToList(const QSqlResult &result,
                                         const QString &what){};
