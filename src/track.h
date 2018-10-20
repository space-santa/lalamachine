#pragma once

#include <QJsonObject>
#include <QString>

class Track {
public:
    Track();
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
