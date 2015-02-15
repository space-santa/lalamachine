#include "config.h"

#include <QFile>
#include <QByteArray>
#include <QVariant>
#include <QJsonDocument>
#include <QJsonObject>
#include <QJsonValue>
#include <QDebug>
#include <QStandardPaths>
#include <QDir>

const QString Config::LALADIR = QStandardPaths::writableLocation(
                                    QStandardPaths::GenericConfigLocation) + "/lalamachine";
const QString Config::CONFIGPATH = Config::LALADIR + "/config.json";
const QString Config::PLAYLISTDIR = Config::LALADIR + "/playlists";
const QString Config::MUSICLIBPATH = Config::LALADIR + "/musiclib.json";

Config::Config(QQuickItem *parent) :
    QQuickItem(parent)
{
    QDir dir(Config::LALADIR);
    dir.mkpath(Config::LALADIR);
    loadConfig();
}

void Config::saveConfig()
{
    saveJsonFile(Config::CONFIGPATH, config_);
}

void Config::loadConfig()
{
    config_ = loadJsonFile(Config::CONFIGPATH);
}

void Config::setVolume(double val)
{
    config_.insert("volume", val);
    emit volumeChanged();
}

double Config::volume()
{
    double retval = 0;
    QJsonValue v = config_.value("volume");

    if (v.isDouble()) {
        retval = v.toDouble();
    }
    else {
        qWarning("No proper volume defined in config.json");
    }

    qDebug() << "Volume loaded" << retval;
    return retval;
}

QJsonObject Config::loadJsonFile(const QString &path)
{
    // Reading the JSON
    QFile loadFile(path);

    if (!loadFile.open(QIODevice::ReadOnly)) {
        qWarning("Couldn't open json file.");
        return QJsonObject();
    }

    QByteArray val;
    val = loadFile.readAll();
    // Create the JsonDocument
    QJsonDocument d = QJsonDocument::fromJson(val);

    if (!d.isObject() || d.isEmpty() || d.isNull()) {
        qWarning("Invalid config file.");
        return QJsonObject();
    }
    else {
        return d.object();
    }
}

void Config::saveJsonFile(const QString &path, const QJsonObject &obj)
{
    QJsonDocument d(obj);
    QFile saveFile(path);
    qDebug() << "Config::setData";

    if (!saveFile.open(QIODevice::WriteOnly)) {
        qWarning("Couldn't open config file.");
        return;
    }

    saveFile.write(d.toJson());
    qDebug() << "Config::dataSet";
}
