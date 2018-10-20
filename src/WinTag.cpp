#include "WinTag.h"
#include <QFile>
#include "timeconverter.h"

#include <QJsonObject>
#include <QDebug>
#include <QJsonArray>
#include <QJsonDocument>
#include <QProcess>
#include <qstring.h>

WinTag::WinTag(const QUrl& url) {
    path_ = url.toLocalFile();
    getTheTag();
}

QString WinTag::album() const {
    return raw_.value("album").toString();
}

QString WinTag::artist() const {
    return raw_.value("artist").toArray().first().toString();
}

QString WinTag::comment() const {
    return raw_.value("comment").toString();
}

QString WinTag::genre() const {
    return raw_.value("genre").toArray().first().toString();
}

QString WinTag::title() const {
    return raw_.value("title").toString();
}

QString WinTag::track() const {
    return raw_.value("track").toString();
}

QString WinTag::year() const {
    return QString::number(raw_.value("year").toInt());
}

QString WinTag::discNumber() const {
    return QString::number(raw_.value("discNumber").toInt());
}

QString WinTag::length() const {
    auto lstr = lengthString();
    auto split1 = lstr.split(":");
    int hours = split1[0].toInt();
    int minutes = split1[1].toInt();
    auto secondsplit = split1[2].split(".");
    int seconds = secondsplit[0].toInt();
    int secondValue = hours * 60 * 60 + minutes * 60 + seconds;
    return QString::number(secondValue);
}

QString WinTag::lengthString() const {
    return raw_.value("duration").toString();
}

QString WinTag::path() const {
    return path_;
}

void WinTag::getTheTag() {
    QProcess gzip;
    gzip.start("C:/Users/claus/code/Qt/lalamachine/dependencies/tag/TheInterface.exe", QStringList() << path_);
    if (!gzip.waitForStarted())
        return;

    if (!gzip.waitForFinished())
        return;

    QByteArray result = gzip.readAll();

    QJsonDocument doc = QJsonDocument::fromJson(result);

    raw_ = doc.object();
}
