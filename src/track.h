#pragma once

#include <QJsonObject>
#include <QString>

/*!
 * \brief The Track class is used to create entries in the PlaylistModel.
 */
class Track {
public:
    Track();
    Track(const QString& track,
          const QString& title,
          const QString& comment,
          const QString& genre,
          const QString& artist,
          const QString& album,
          const QString& mrl,
          const QString& path,
          const QString& length,
          const QString& lengthString,
          const QString& year,
          const QString& id = 0);
    Track(const QJsonObject& json);

    void fromJson(const QJsonObject& json);
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
    int disc_;
    int id_;
};
