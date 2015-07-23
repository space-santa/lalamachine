#ifndef TRACK_H
#define TRACK_H

#include <QString>
#include <QJsonObject>

class Track
{
public:
    Track();
    Track(const QString &track,
          const QString &title,
          const QString &comment,
          const QString &genre,
          const QString &artist,
          const QString &album,
          const QString &mrl,
          const QString &path,
          const QString &length,
          const QString &lengthString,
          const QString &year,
          const QString &dateAdded,
          const QString &id = 0);
    Track(const QJsonObject &json);

    void fromJson(const QJsonObject &json);
    QJsonObject toJson() const;

    int track_;
    QString title_;
    QString comment_;
    QString genre_;
    QString artist_;
    QString album_;
    QString mrl_;
    QString path_;
    int length_;
    QString lengthString_;
    int year_;
    QString dateAdded_;
    int id_;
};

#endif  // TRACK_H
