#pragma once

#include <tag.h>
#include <QJsonObject>
#include <QString>
#include <QUrl>

// TODO: Make this a child of TagLib::Tag. It just adds toJson()
// functionality, path and lengthString.
class Tags {
 public:
  Tags();
  Tags(const TagLib::Tag *tag, const QString &path, const QString &mrl,
       int length, QString lengthString, uint discNumber);

  QString album_;
  QString artist_;
  QString comment_;
  QString genre_;
  QString length_;
  QString lengthString_;
  QString mrl_;
  QString path_;
  QString title_;
  QString track_;
  QString year_;
  QString disc_;

  QJsonObject toJson();

  bool isValid();
};
