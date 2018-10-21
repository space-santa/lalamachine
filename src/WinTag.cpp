#include "WinTag.h"
#include <QFile>
#include <stdexcept>
#include "timeconverter.h"

#include <QDebug>
#include <QJsonArray>
#include <QJsonDocument>
#include <QJsonObject>
#include <QProcess>
#include <QRegularExpression>
#include <QString>

#ifdef _WIN32
#include <Windows.h>
#endif

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

int WinTag::track() const {
    return raw_.value("track").toInt();
}

int WinTag::year() const {
    return raw_.value("year").toInt();
}

int WinTag::discNumber() const {
    return raw_.value("discNumber").toInt();
}

int WinTag::length() const {
    auto lstr = lengthString();
    auto split1 = lstr.split(":");
    int minutes = split1[0].toInt();
    int seconds = split1[1].toInt();
    int secondTotal = minutes * 60 + seconds;
    return secondTotal;
}

QString WinTag::lengthString() const {
    auto lstr = raw_.value("duration").toString();
    auto split1 = lstr.split(":");
    int hours = split1[0].toInt();
    int minutes = split1[1].toInt();
    auto secondsplit = split1[2].split(".");
    int seconds = secondsplit[0].toInt();
    int minutesValue = hours * 60 * 60 + minutes;
    QString secondsString = QString::number(seconds);
    if (secondsString.length() == 1) {
        secondsString.prepend("0");
    }
    return QString::number(minutesValue) + ":" + secondsString;
}

QString WinTag::path() const {
    return path_;
}

QString WinTag::execPath() const {
#ifdef _WIN32
    auto hModule = GetModuleHandleW(NULL);
    WCHAR modulePath[MAX_PATH];
    GetModuleFileNameW(hModule, modulePath, MAX_PATH);
    QString execPath = QString::fromWCharArray(modulePath);
    execPath.remove(QRegularExpression("lalamachine.exe$"));
    return execPath + "TheInterface.exe";
#else
    throw std::runtime_error("This must not happen. This class only makes sense on Windows.");
#endif
}

void WinTag::getTheTag() {
    QProcess gzip;
    gzip.start(execPath(), QStringList() << path_);
    if (!gzip.waitForStarted()) {
        return;
    }

    if (!gzip.waitForFinished()) {
        return;
    }

    QByteArray result = gzip.readAll();
    QString str = QString::fromLocal8Bit(result);
    QJsonParseError err;
    QJsonDocument doc = QJsonDocument::fromJson(str.toUtf8(), &err);
    raw_ = doc.object();
}
