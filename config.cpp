/*
Copyright 2015 Armin Zirkel

This file is part of lalamachine.

Lalamachine is free software: you can redistribute it and/or modify
it under the terms of the GNU General Public License as published by
the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

Lalamachine is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU General Public License for more details.

You should have received a copy of the GNU General Public License
along with lalamachine.  If not, see <http://www.gnu.org/licenses/>.
*/

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

    // If we want the utf8 encoding to be correctly written to the file,
    // we cannot write the JSON document directly. We must write an encoded
    // QString into the file.
    QTextStream out(&saveFile);
    out << QString::fromUtf8(d.toJson());
    qDebug() << "Config::dataSet";
}
