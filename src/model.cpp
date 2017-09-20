#include "model.h"

#include "config.h"

Model::Model() {}

void Model::init() {
  db_ = QSqlDatabase::addDatabase("QSQLITE");
  db_.setDatabaseName(Config::MUSICLIBDB);
  db_.open();
}

QStringList Model::genre(const QString &filter) {
  QSqlResult result = db.exec(Model::genreQuery(filter));
  return Model::resultToList(result, "genre");
}

QString Model::genreQuery(const QString &filter) {
  QString query("SELECT DISTINCT genre FROM musiclib %1 ORDER BY genre ASC");

  if (!filter().isEmpty()) {
    query = query.arg("WHERE UPPER(genre) LIKE '%%1%'")
                .arg(Model::escapeString(filter().toUpper()));
  } else {
    query = query.arg("");
  }

  return query;
}

QString Model::escapeString(QString str) {
  // return str.replace("\'", "\'\'").replace(",", "\'+\',\'+\'");
  return str.replace("\'", "\'\'");
}

QStringList Model::resultToList(const QSqlResult &result, const QString &what) {
  QStringList retval;

  while (result.next()) {
    QString tmp = result.value(what).toString();

    if (!tmp.isEmpty()) {
      retval << tmp;
    }
  }

  return retval;
}
